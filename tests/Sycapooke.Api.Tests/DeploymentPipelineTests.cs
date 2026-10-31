// Architecture Test
// Traces to: L2-084, L2-085, L2-086, L2-087
// Description: The two deploy workflows, compose stack and Caddyfiles deploy only green pushes to main, deploy the
// marketing site and the application independently, migrate before the application restart, fail on a bad health
// check, keep secrets scoped, fit a small VM and never cache index.html.

using System.Text.RegularExpressions;

namespace Sycapooke.Api.Tests;

public sealed partial class DeploymentPipelineTests
{
    private static readonly string Root = FindRepositoryRoot();
    private static readonly string MarketingWorkflow = Read(".github/workflows/deploy-marketing.yml");
    private static readonly string AppWorkflow = Read(".github/workflows/deploy-app.yml");
    private static readonly string Compose = Read("deploy/docker-compose.yml");
    private static readonly string EdgeCaddyfile = Read("deploy/edge/Caddyfile");
    private static readonly string MarketingCaddyfile = Read("deploy/marketing/Caddyfile");
    private static readonly string AppCaddyfile = Read("deploy/app/Caddyfile");
    private static readonly string MarketingDockerfile = Read("deploy/marketing/Dockerfile");
    private static readonly string AppDockerfile = Read("deploy/app/Dockerfile");

    public static TheoryData<string> Workflows => new() { MarketingWorkflow, AppWorkflow };

    [Theory]
    [MemberData(nameof(Workflows))]
    public void Each_deploy_runs_after_a_successful_ci_push_to_main(string workflow)
    {
        Assert.Matches(@"workflow_run:\s*\n\s+workflows: \[CI\]\s*\n\s+types: \[completed\]\s*\n\s+branches: \[main\]", workflow);
        Assert.Contains("github.event.workflow_run.conclusion == 'success'", workflow);
        Assert.Contains("github.event.workflow_run.event == 'push'", workflow);
        Assert.DoesNotMatch(@"\n  push:", workflow);
    }

    [Theory]
    [MemberData(nameof(Workflows))]
    public void Each_deploy_can_be_run_by_hand_for_any_sha_as_a_rollback(string workflow)
    {
        Assert.Contains("workflow_dispatch:", workflow);
        Assert.Matches(@"inputs:\s*\n\s+ref:", workflow);
        Assert.Contains("inputs.ref || github.event.workflow_run.head_sha || github.sha", workflow);
        Assert.Contains("ref: ${{ env.DEPLOY_SHA }}", workflow);
    }

    [Fact]
    public void Each_workflow_moves_only_its_own_tag_and_services()
    {
        Assert.Contains("MARKETING_TAG=${{ env.DEPLOY_SHA }}", MarketingWorkflow);
        Assert.DoesNotContain("APP_TAG=", MarketingWorkflow);
        Assert.Contains("docker compose up -d --no-deps marketing edge", MarketingWorkflow);
        Assert.DoesNotContain("migrator", MarketingWorkflow);

        Assert.Contains("APP_TAG=${{ env.DEPLOY_SHA }}", AppWorkflow);
        Assert.DoesNotContain("MARKETING_TAG=", AppWorkflow);
        Assert.Contains("docker compose up -d --no-deps api worker app edge", AppWorkflow);
        Assert.DoesNotContain(" marketing", AppWorkflow[AppWorkflow.IndexOf("bash -s <<'REMOTE'", StringComparison.Ordinal)..]);
    }

    [Fact]
    public void Deploys_of_the_same_unit_never_overlap_but_the_two_units_are_independent()
    {
        Assert.Matches(@"concurrency:\s*\n\s+group: deploy-marketing\s*\n\s+cancel-in-progress: false", MarketingWorkflow);
        Assert.Matches(@"concurrency:\s*\n\s+group: deploy-app\s*\n\s+cancel-in-progress: false", AppWorkflow);
    }

    [Fact]
    public void Migrations_run_before_the_application_restart_and_a_failure_stops_it()
    {
        var script = AppWorkflow[AppWorkflow.IndexOf("bash -s <<'REMOTE'", StringComparison.Ordinal)..];
        var strict = script.IndexOf("set -euo pipefail", StringComparison.Ordinal);
        var migrate = script.IndexOf("docker compose run --rm -T migrator < /dev/null", StringComparison.Ordinal);
        var restart = script.IndexOf("docker compose up -d", StringComparison.Ordinal);

        Assert.True(strict >= 0 && migrate > strict && restart > migrate, "Expected set -euo pipefail, then the migrator, then up -d.");
        Assert.Contains("set -euo pipefail", MarketingWorkflow);
    }

    [Fact]
    public void A_failed_smoke_test_fails_the_run()
    {
        Assert.Contains("curl -fsS \"https://${SITE_HOST}/api/health\" && exit 0", AppWorkflow);
        Assert.Contains("echo \"health check failed\" && exit 1", AppWorkflow);
        Assert.Contains("curl -fsS -o /dev/null \"https://${SITE_HOST}/\" && exit 0", AppWorkflow);
        Assert.Contains("grep -q '<base href=\"/welcome/\">' marketing.html && exit 0", MarketingWorkflow);
        Assert.Contains("echo \"marketing check failed\" && exit 1", MarketingWorkflow);
    }

    [Theory]
    [MemberData(nameof(Workflows))]
    public void Only_the_build_job_may_write_packages_and_deploy_uses_the_production_environment(string workflow)
    {
        Assert.Single(PackagesWrite().Matches(workflow));
        var build = Job(workflow, "build");
        var deploy = Job(workflow, "deploy");

        Assert.Contains("packages: write", build);
        Assert.Contains("environment: production", deploy);
        Assert.DoesNotContain("packages: write", deploy);
        Assert.Matches(@"^permissions: \{\}", workflow.Split('\n').First(line => line.StartsWith("permissions:", StringComparison.Ordinal)));
    }

    [Theory]
    [MemberData(nameof(Workflows))]
    public void Secrets_are_never_echoed_and_the_key_is_private(string workflow)
    {
        Assert.DoesNotMatch(@"echo[^\n]*secrets\.", workflow);
        Assert.Contains("umask 077", workflow);
        Assert.Contains("--password-stdin", workflow);
    }

    [Theory]
    [MemberData(nameof(Workflows))]
    public void Each_deploy_sets_the_Azure_site_host_and_serializes_shared_mutations(string workflow)
    {
        Assert.Contains("printf 'SITE_ADDRESS=%s\\n'", workflow);
        Assert.Contains("scp deploy/edge/Caddyfile", workflow);
        Assert.Contains("exec 9>.deploy.lock", workflow);
        Assert.Contains("flock -x 9", workflow);
        Assert.Contains("export DOCKER_CONFIG=/tmp/sycapooke-registry-${{ github.run_id }}", workflow);
        Assert.DoesNotContain("https://app.${SITE_HOST}", workflow);
        Assert.DoesNotContain("https://www.${SITE_HOST}", workflow);
    }

    [Theory]
    [InlineData("edge", "64m")]
    [InlineData("marketing", "64m")]
    [InlineData("app", "64m")]
    [InlineData("api", "512m")]
    [InlineData("worker", "384m")]
    [InlineData("migrator", "384m")]
    public void Each_service_fits_a_small_vm_with_rotated_logs(string service, string memory)
    {
        var block = Service(service);

        Assert.Contains($"mem_limit: {memory}", block);
        Assert.Contains("logging: *logging", block);
        Assert.DoesNotContain("replicas", block);
    }

    [Fact]
    public void The_marketing_site_and_the_application_are_separate_images_with_their_own_tags()
    {
        Assert.Contains("sycapooke-marketing:${MARKETING_TAG:-latest}", Service("marketing"));
        Assert.Contains("sycapooke-app:${APP_TAG:-latest}", Service("app"));
        Assert.Contains("sycapooke-api:${APP_TAG:-latest}", Service("api"));
        Assert.Contains("sycapooke-worker:${APP_TAG:-latest}", Service("worker"));
        Assert.Contains("sycapooke-migrator:${APP_TAG:-latest}", Service("migrator"));
        Assert.Contains("npm run build:marketing", MarketingDockerfile);
        Assert.Contains("COPY --from=build /src/dist/sycapooke/browser /srv/marketing", MarketingDockerfile);
        Assert.Contains("npm run build:all", AppDockerfile);
        Assert.Contains("COPY --from=build /src/dist/sycapooke/browser /srv/app", AppDockerfile);
    }

    [Fact]
    public void Logs_rotate_through_json_file()
    {
        Assert.Matches(@"x-logging: &logging\s*\n\s+driver: json-file\s*\n\s+options:\s*\n\s+max-size: ""10m""\s*\n\s+max-file: ""3""", Compose);
    }

    [Fact]
    public void Index_html_and_spa_routes_are_not_cached_but_hashed_assets_are()
    {
        foreach (var caddyfile in new[] { MarketingCaddyfile, AppCaddyfile })
        {
            Assert.Contains("@entry not path *.js *.css", caddyfile);
            Assert.Contains("header @entry Cache-Control \"no-cache\"", caddyfile);
            Assert.Contains("try_files {path} /index.html", caddyfile);
        }
    }

    [Fact]
    public void The_edge_serves_both_builds_on_the_Azure_host_and_only_the_application_proxies_the_api()
    {
        Assert.Contains("redir /welcome /welcome/ permanent", EdgeCaddyfile);
        Assert.Matches(@"handle_path /welcome/\* \{\s*\n\s+reverse_proxy marketing:80", EdgeCaddyfile);
        Assert.Matches(@"handle \{\s*\n\s+reverse_proxy app:80", EdgeCaddyfile);
        Assert.Contains("--base-href /welcome/", MarketingDockerfile);
        Assert.Matches(@"handle /api/\* \{\s*\n\s+reverse_proxy api:8080", AppCaddyfile);
        Assert.Matches(@"handle /photos/\* \{\s*\n\s+reverse_proxy api:8080", AppCaddyfile);
        Assert.DoesNotContain("reverse_proxy", MarketingCaddyfile);
        Assert.Contains("./edge/Caddyfile:/etc/caddy/Caddyfile:ro", Service("edge"));
    }

    private static string Job(string workflow, string name)
    {
        var start = workflow.IndexOf($"\n  {name}:\n", StringComparison.Ordinal);
        Assert.True(start >= 0, $"Job {name} not found.");
        var next = TopLevelKey().Match(workflow, start + 1);
        return workflow[start..(next.Success ? next.Index : workflow.Length)];
    }

    private static string Service(string name)
    {
        var start = Compose.IndexOf($"\n  {name}:\n", StringComparison.Ordinal);
        Assert.True(start >= 0, $"Service {name} not found.");
        var next = TopLevelKey().Match(Compose, start + 1);
        return Compose[start..(next.Success ? next.Index : Compose.Length)];
    }

    private static string Read(string path) => File.ReadAllText(Path.Combine(Root, path)).ReplaceLineEndings("\n");

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "AGENTS.md")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Repository root (AGENTS.md) not found.");
    }

    [GeneratedRegex(@"\n  [a-z-]+:\n|\n[a-z]")]
    private static partial Regex TopLevelKey();

    [GeneratedRegex("packages: write")]
    private static partial Regex PackagesWrite();
}
