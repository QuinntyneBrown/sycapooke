import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

import { IconComponent } from '../icon/icon';

/**
 * A video's poster as one play button in the hero photo frame (design system: Video poster): the
 * 4:5 photo with a cream pill bottom-left. It renders no video, so nothing plays until `watch` is
 * handled, typically by opening the video dialog. The accessible name is `label` only; the
 * duration is decorative.
 */
@Component({
  selector: 'sy-video-poster',
  imports: [IconComponent],
  templateUrl: './video-poster.html',
  styleUrl: './video-poster.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VideoPosterComponent {
  readonly poster = input.required<string>();
  readonly label = input.required<string>();
  /** Length shown in the pill, such as 0:29. */
  readonly duration = input('');
  readonly watch = output();
}
