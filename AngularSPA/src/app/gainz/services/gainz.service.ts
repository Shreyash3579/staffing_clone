import { Injectable } from '@angular/core';
import {Observable, of} from "rxjs";

@Injectable({
  providedIn: 'root'
})
export class GainzService {

  constructor() { }
  getChats(): Observable<any> {
    const chatsData = require("../mocked/chats.json");
    return of(chatsData);
  }
}
