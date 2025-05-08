import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class HomeHelperService {
  private collapseNewDemandsAll : Subject<Boolean> = new Subject(); // used as Subject since we don't need initial value
  constructor() { }

  setCollapseNewDemandAll(isCollapsed: boolean) {
    this.collapseNewDemandsAll.next(isCollapsed);
  }

  getCollapseNewDemandAll() {
    return this.collapseNewDemandsAll.asObservable();
  }
}
