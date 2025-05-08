import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class CasePlanningPlaygroundService {
  private playgroundId$ = new BehaviorSubject<any>(null);
  selectedPlaygroundId$ = this.playgroundId$.asObservable();

  constructor() { }

  setPlaygroundId(playgroundId: any) {
    this.playgroundId$.next(playgroundId);
  }
}
