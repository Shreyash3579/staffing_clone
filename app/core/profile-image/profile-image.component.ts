import { Component, Input, input } from '@angular/core';
import { ImageFallBackDirective } from 'src/app/shared/directives/image-fall-back.directive';

@Component({
  selector: 'app-profile-image',
  standalone: true,
  imports: [ImageFallBackDirective],
  templateUrl: './profile-image.component.html',
  styleUrl: './profile-image.component.scss'
})
export class ProfileImageComponent {
  @Input() imageUrl: string;
  @Input() width: number = 25;
  @Input() height: number = 25;

}
