import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Observable, Observer, forkJoin, Subject, AsyncSubject } from 'rxjs';

@Injectable()
export class ProfileImageService {
    public defaultNoImgUrl = 'assets/img/user-icon.jpg';
    public imgUrl: Subject<string> = new AsyncSubject();
    fallbackIndex; number : 0;

    // getImage(profileImgUrl: string) {
    // if (this.fallbackIndex < environment.imageExtensions.length) {
    //     this.currentImageUrl = environment.imageExtensions[this.fallbackIndex];
    //     this.fallbackIndex++;
    //   } else {
    //     console.error('All image URLs failed to load.');
    //   }
    // }

    getImage(profileImgUrl: string) {
        try {
            const extensions = environment.imageExtensions;
            let imageExists = false;
            const probableImageUrls = [];
            extensions.forEach(function (extension) {
                const probableImageUrl = profileImgUrl.replace('.jpg', extension);
                probableImageUrls.push(probableImageUrl);
            });
            const observables = probableImageUrls.map(probableImageUrl => this.checkImage(probableImageUrl));
            const source = forkJoin(observables);
            source.subscribe(response => {
                for (let i = 0; i < response.length; ++i) {
                  if (response[i]) {
                    imageExists = true;
                    this.imgUrl.next(probableImageUrls[i]);
                  }
                }
                if (!imageExists) {
                    this.imgUrl.next(this.defaultNoImgUrl);
                }
                // this.imgUrl.complete();
              });
        } catch (e) {
            this.imgUrl.next(this.defaultNoImgUrl);
            // this.imgUrl.complete();
        }
    }

    private checkImage(src): Observable<Boolean> {
        return new Observable((observer: Observer<boolean>) => {
        // return Observable.create((observer: Observer<boolean>) => {
            const img = new Image();
            img.src = src;
            if (!img.complete) {
                img.onload = () => {
                    observer.next(true);
                    observer.complete();
                };
                img.onerror = (err) => {
                    observer.next(false);
                    observer.complete();
                };
            } else {
                observer.next(true);
                observer.complete();
            }
        });
    }
}
