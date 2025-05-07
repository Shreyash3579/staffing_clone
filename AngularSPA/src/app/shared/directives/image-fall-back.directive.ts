import { Directive, ElementRef, HostListener, Input, OnInit, Renderer2 } from '@angular/core';
import { environment } from 'src/environments/environment';

@Directive({
  selector: 'img[appImageFallBack]',
  standalone: true
})
export class ImageFallBackDirective {
  public defaultNoImgUrl = 'assets/img/user-icon.jpg';
  extensions = environment.imageExtensions;
  probableImageUrls: string[] = [];

  @Input() profileImageUrl: string;

  private currentFallbackIndex: number = 0;

  //The entire commented code is for showing user icon till actual image loads. However, drawback is that it will resulst in additional call per image to identify if image exists f not
  // constructor(private el: ElementRef, private renderer: Renderer2) {}

  // ngOnInit(): void {
  //   // Set the default icon initially
  //   this.renderer.setAttribute(this.el.nativeElement, 'src', "assets/img/user-icon.jpg");

  //   // Attempt to load the primary image
  //   if (this.profileImageUrl) {
  //     this.loadImage(this.profileImageUrl).then(() => {
  //       this.renderer.setAttribute(this.el.nativeElement, 'src', this.profileImageUrl);
  //     }).catch(() => {
  //       this.loadNextFallbackImage();
  //     });
  //   }
  // }

  // private loadImage(src: string): Promise<void> {
  //   return new Promise((resolve, reject) => {
  //     const img = new Image();
  //     img.src = src;
  //     img.onload = () => resolve();
  //     img.onerror = () => reject();
  //   });
  // }

  // private loadNextFallbackImage() {
  //   if (this.currentFallbackIndex < this.extensions.length) {
  //   this.profileImageUrl = this.profileImageUrl.replace('.jpg', this.extensions[this.currentFallbackIndex]);
  //   // if (this.currentFallbackIndex < this.extensions.length) {
  //   //   element.src = this.profileImageUrl.replace('.jpg', this.extensions[this.currentFallbackIndex]);
  //   //   this.currentFallbackIndex++;
  //   // }else{
  //   //   element.src = this.defaultNoImgUrl;
  //   // }
  //    this.loadImage(this.profileImageUrl).then(() => {
  //       this.renderer.setAttribute(this.el.nativeElement, 'src', this.profileImageUrl);
  //     }).catch(() => {
  //       this.currentFallbackIndex++;
  //       this.loadNextFallbackImage();
  //     });
  //   }
  // }
  
  @HostListener('error', ['$event'])
  onError(event: Event) {
    const element = event.target as HTMLImageElement;
    if (this.currentFallbackIndex < this.extensions.length) {
      element.src = this.profileImageUrl.replace('.jpg', this.extensions[this.currentFallbackIndex]);
      this.currentFallbackIndex++;
    }else{
      element.src = this.defaultNoImgUrl;
    }
  }


}
