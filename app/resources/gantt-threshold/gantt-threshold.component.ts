import { Component, Input, OnChanges, OnInit, SimpleChanges } from '@angular/core';
import { WeeklyMonthlyGroupingEnum } from 'src/app/shared/constants/enumMaster';

@Component({
  selector: 'app-gantt-threshold',
  templateUrl: './gantt-threshold.component.html',
  styleUrls: ['./gantt-threshold.component.scss']
})
export class GanttThresholdComponent implements OnInit, OnChanges {
  public mergedArray = [];
  public gridSpace = 2;

  @Input() perDayAllocation: any;
  @Input() thresholdRangeValue: any;
  @Input() selectedWeeklyMonthlyGroupingOption: string;

  public readonly weeklyMonthlyGroupingEnum = WeeklyMonthlyGroupingEnum;

  constructor() { }

  ngOnInit(): void {
  }

  ngOnChanges(changes: SimpleChanges): void{
    if(((changes.perDayAllocation || changes.thresholdRangeValue) && this.perDayAllocation && this.thresholdRangeValue)
      || changes.selectedWeeklyMonthlyGroupingOption) {
      this.setThresholdClasses()
    }
  }

  getClass (item): any {
    const baseClass = this.selectedWeeklyMonthlyGroupingOption === this.weeklyMonthlyGroupingEnum.MONTHLY ? 
    'threshold-monthly-range' : 'threshold-range';
    const additionalClass = item.className ? item.className : 'start-0 duration-0';
    return {
      [baseClass]: true,
      [additionalClass]: additionalClass !== ''
    };
  }


  private setThresholdClasses(){
    
    if(!this.thresholdRangeValue.isFilterApplied){
      this.mergedArray =[];
      return;
    }
     
    let lastAllocationValueInBucket = -1;
    let className = "";
    this.mergedArray =[];
    for(let i = 0; i< this.perDayAllocation.length -1; i++){
      const isperDayAllocationOutsideThresholdRange = (this.perDayAllocation[i] <= this.thresholdRangeValue.min || this.perDayAllocation[i] > this.thresholdRangeValue.max) ;
      
      if(isperDayAllocationOutsideThresholdRange && this.perDayAllocation[i] != lastAllocationValueInBucket){
        lastAllocationValueInBucket = this.perDayAllocation[i];

        for(let j = i+1; j<= this.perDayAllocation.length; j++){

          if(this.selectedWeeklyMonthlyGroupingOption == this.weeklyMonthlyGroupingEnum.WEEKLY){
            className = `start-${i+1} duration-${(j-i)}`;
           } else {
            className = `start-${i+1}-${this.gridSpace} duration-${(j-i)}-${this.gridSpace}`;
           }
          if(this.perDayAllocation[j] != lastAllocationValueInBucket){

            this.mergedArray.push({className: className, allocation: lastAllocationValueInBucket});
            
            i=j-1;
            break;
          }
            
        }
        
      }
    }
  }

}
