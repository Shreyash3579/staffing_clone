import { Injectable } from '@angular/core';
import * as moment from 'moment';

@Injectable({
  providedIn: 'root'
})
export class LocalStorageService {

  constructor() { }

  set(key: string, data: any, expirationFrequency: string = 'daily'): void {
    if (expirationFrequency === 'daily') {

      const expiresAt = moment().add(1, 'day').startOf('day');
      localStorage.setItem('daily_expires_at', JSON.stringify(expiresAt.valueOf()) );

    } else if (expirationFrequency === 'weekly') {

      const expiresAt = moment().add(1, 'week').startOf('day');
      localStorage.setItem('weekly_expires_at', JSON.stringify(expiresAt.valueOf()) );

    } else if (expirationFrequency === 'monthly') {

      const expiresAt = moment().add(1, 'month').startOf('day');
      localStorage.setItem('monthly_expires_at', JSON.stringify(expiresAt.valueOf()) );

    }

    localStorage.setItem(key, JSON.stringify(data));
  }

  removeItem(key:string){
    localStorage.removeItem(key);
  }

  get(key: string, expirationFrequency: string = 'daily') {
    // if (moment().isAfter(this.getExpiration(expirationFrequency))) {
    //     return null;
    // }

    return JSON.parse(localStorage.getItem(key));

  }

  getExpiration(expirationFrequency) {
    let expiration;
    if (expirationFrequency === 'daily') {

      expiration = localStorage.getItem('daily_expires_at');

    } else if (expirationFrequency === 'monthly') {

      expiration = localStorage.getItem('monthly_expires_at');

    }

    // if expiration para,eters are not set, then set expiration for previous day so that expiration parameters can be set
    // and local storage is refreshed
    if (expiration == null) {
      expiration = moment().add(-1, 'day').startOf('day').valueOf();
    }

    const expiresAt = JSON.parse(expiration);
    return moment(expiresAt);
  }

}
