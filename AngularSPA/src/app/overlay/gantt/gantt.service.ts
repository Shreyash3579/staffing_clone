import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { ResourceAllocation } from 'src/app/shared/interfaces/resourceAllocation.interface';

@Injectable({
  providedIn: 'root'
})
export class GanttService {

  public resourceAndPlaceholderAllocations: BehaviorSubject<ResourceAllocation[]> = new BehaviorSubject<ResourceAllocation[]>(null);
  public resourceCommitments: BehaviorSubject<any> = new BehaviorSubject(null);
  constructor() { }
}
