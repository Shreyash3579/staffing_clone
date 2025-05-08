import { Injectable } from '@angular/core';
import { NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';

@Injectable({
  providedIn: 'root',
})
export class UrlService {
  redirectURL: string = null;

  constructor() { }

  setRedirectUrlTabName(previousOpenedTab: string) {
    this.redirectURL = previousOpenedTab;
  }

  setRedirectUrl(router) {
    router.events.pipe(
      filter((event) => event instanceof NavigationEnd)
    ).subscribe((event: NavigationEnd) => {
      if (event.url && !event.url.includes('?')) {
        this.setRedirectUrlTabName(event.url);
      }
    });
  }
}
