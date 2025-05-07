import { Component,ElementRef, EventEmitter, Input, OnInit, Output, Renderer2, ViewChild } from '@angular/core';
import * as moment from 'moment';
import { ConstantsMaster } from 'src/app/shared/constants/constantsMaster';
import { ProjectBackgroundColorCode, ProjectBorderColorCode, ProjectType } from 'src/app/shared/constants/enumMaster';
import { DateService } from 'src/app/shared/dateService';
import { PlanningCard } from 'src/app/shared/interfaces/planningCard.interface';
import { Project } from 'src/app/shared/interfaces/project.interface';
import { NotificationService } from 'src/app/shared/notification.service';
import { ValidationService } from 'src/app/shared/validationService';

@Component({
  selector: 'app-projects-gantt-project',
  templateUrl: './projects-gantt-project.component.html',
  styleUrls: ['./projects-gantt-project.component.scss']
})
export class ProjectsGanttProjectComponent implements OnInit {

  constructor(
    private notifyService: NotificationService,
    private render: Renderer2) { }

  // ---------------- Input Events -------------------- //
  @Input() casesGanttData: Project;
  @Input() planningCard: PlanningCard;
  @Input() dateRange: [Date, Date];
  
  @Input() bounds: HTMLElement;
  @Input() caseIndex: number;

  // -----------------Output Events ------------------- //
  @Output() openCaseRollForm = new EventEmitter<any>();
  @Output() showQuickPeekDialog = new EventEmitter();
  @Output() openPlaceholderForm = new EventEmitter<any>();
  @Output() openCaseDetailsDialog = new EventEmitter();
  @Output() openAddTeamSkuForm = new EventEmitter<any>();
  
  // -----------------View Child Refs ------------------- //
  @ViewChild('leftGanttData') ganttCase : ElementRef;
  @ViewChild('ganttPlanningCard') ganttPlanningCard : ElementRef;

  // ------------------------Local Variables---------------------------------------

  gridSize = 45;
  offsetLeft;
  gridCellMargin = 6;
  className = '';
  public commitmentDurationInDays: any;
  clickTypeIndicator: any;
  projectDetails = { projectName: "", projectType: "", startDate: "", endDate: "", officeAbbreviation: "", isCaseRoll: false, skuTerm : '' }
  activeResourcesEmailAddresses:string = "";
  accessibleFeatures = ConstantsMaster.appScreens.feature;
  // ------------------------Life Cycle Events---------------------------------------

  ngOnInit() {
    this.setProjectDetails();
    this.setCommitmentClasses();
    this.getActiveResourcesEmailAddress();
  }

  private setProjectDetails() {
    if (this.casesGanttData) {
      this.setProjectTitle();
      this.projectDetails.projectType = this.casesGanttData.type;
      this.projectDetails.startDate = this.casesGanttData.startDate;
      this.projectDetails.endDate = this.casesGanttData.endDate;
      this.projectDetails.officeAbbreviation = this.casesGanttData.managingOfficeAbbreviation;
      this.projectDetails.isCaseRoll = this.casesGanttData.caseRoll ? true : false;
      this.projectDetails.skuTerm = this.casesGanttData.skuCaseTerms?.skuTerms?.map(s => s.name).toString() ?? '';
    }
    else if (this.planningCard) {
      this.projectDetails.projectType = ProjectType.PlanningCard;
      this.projectDetails.projectName = this.planningCard.name;
      this.projectDetails.startDate = DateService.convertDateInBainFormat(this.planningCard.startDate);
      this.projectDetails.endDate = DateService.convertDateInBainFormat(this.planningCard.endDate);
    }
  }

  // ------------------------Helper Functions-----------------------------------

  private setProjectTitle() {
    switch (this.casesGanttData.type) {
      case ProjectType.Opportunity: {
        if (this.casesGanttData.probabilityPercent) {
          this.projectDetails.projectName = `${this.casesGanttData.probabilityPercent}% - ${this.casesGanttData.clientName} - ${this.casesGanttData.opportunityName}`;
        } else {
          this.projectDetails.projectName = `${this.casesGanttData.clientName} - ${this.casesGanttData.opportunityName}`;
        }

        break;
      }
      default: {
        this.projectDetails.projectName = `${this.casesGanttData.oldCaseCode} - ${this.casesGanttData.clientName} - ${this.casesGanttData.caseName}`;
        break;
      }
    }
  }

  private setCommitmentClasses(reAdjustClass = false, $event = null) {

    let startCount = 0;
    const commitmentStartDate = this.casesGanttData
      ? moment(this.casesGanttData.startDate).startOf('day')
      : moment(this.planningCard.startDate).startOf('day');
    const dateRangeStartDate = moment(this.dateRange[0]).startOf('day');
    const commitmentEndDate = this.casesGanttData
      ? moment(this.casesGanttData.endDate).startOf('day')
      : moment(this.planningCard.endDate).startOf('day');
    const dateRangeEndDate = moment(this.dateRange[1]).startOf('day');
    this.commitmentDurationInDays = commitmentEndDate.diff(commitmentStartDate, 'days');

    if (commitmentStartDate.isAfter(dateRangeStartDate)) {
      startCount = commitmentStartDate.diff(dateRangeStartDate, 'days') + 1;
    }

    const end = commitmentEndDate.isAfter(dateRangeEndDate) ? dateRangeEndDate : commitmentEndDate;
    const start = commitmentStartDate.isAfter(dateRangeStartDate) ? commitmentStartDate : dateRangeStartDate;
    const duration = end.diff(start, 'days') + 1;

    if (reAdjustClass) {
      this.offsetLeft = ((startCount - 1) * this.gridSize + this.gridCellMargin);
      $event.host.classList.add(`duration-${duration}`);
    }

    if(commitmentEndDate >= dateRangeStartDate && commitmentStartDate <= dateRangeEndDate)
    {

      this.className = 'start-' + startCount + ' duration-' + duration;

      if (this.casesGanttData)
        this.className += ' commitment-' + this.getCommitmentColor();

      if(this.planningCard)
        this.className += ' commitment case-placeholder';
    }

  }

  private getCommitmentColor(): string {

    let color = '';

    switch (this.projectDetails.projectType) {
      case ProjectType.Opportunity:
        color = 'yellow';
        break;

      case ProjectType.NewDemand:
        color = 'orange';
        break;

      default: {
        color = 'blue';
        break;
      }
    }

    return color;
  }

  getActiveResourcesEmailAddress() {
    this.activeResourcesEmailAddresses = '';

    if (this.casesGanttData?.allocatedResources) {
      this.casesGanttData.allocatedResources.forEach(resource => {
        if (!this.activeResourcesEmailAddresses.includes(resource.internetAddress)) {
          this.activeResourcesEmailAddresses += resource.internetAddress + ';';
        }
      });
    }else if (this.planningCard?.regularAllocations) {
      this.planningCard.regularAllocations.forEach(resource => {
        if (resource.employeeCode && !this.activeResourcesEmailAddresses.includes(resource.internetAddress)) {
            this.activeResourcesEmailAddresses += resource.internetAddress + ';';
        }
      });
    }
  }

  onCaseRollHandler() {
    if (!ValidationService.isCaseEligibleForRoll(this.casesGanttData.endDate)) {
      this.notifyService.showValidationMsg(ValidationService.caseRollNotAllowedForInActiveCasesMessage);
    } else {
      this.openCaseRollForm.emit({ project: this.casesGanttData });
    }
  }

  quickPeekIntoResourcesCommitmentsHandler() {
    let employees;
    if(this.casesGanttData){
      employees = this.casesGanttData.allocatedResources?.map(x => {
        return {
          employeeCode: x.employeeCode,
          employeeName: x.employeeName,
          levelGrade: x.currentLevelGrade
        };
      });
      
    }else if(this.planningCard){
      employees = this.planningCard.regularAllocations?.map(x => {
        return {
          employeeCode: x.employeeCode,
          employeeName: x.employeeName,
          levelGrade: x.currentLevelGrade
        };
      });
    }

    this.showQuickPeekDialog.emit(employees);
  }

  openPlaceholderFormhandler() {
    if(this.casesGanttData){
      this.openPlaceholderForm.emit({
          project: this.casesGanttData,
          placeholderAllocationData: null,
          isUpdateModal: false
      });
    }else if(this.planningCard){
      this.openPlaceholderForm.emit({
        planningCardData: this.planningCard,
        placeholderAllocationData: null,
        isUpdateModal: false
    });
    }
    
  }

  openAddTeamSkuFormHandler(){
    this.openAddTeamSkuForm.emit(this.casesGanttData);
  }

  addPlaceholder(event) {
    if(this.planningCard)
    {
      this.openPlaceholderFormhandler();
    } else {
      this.openAddTeamSkuFormHandler();
    }
  }

  public openCaseDetailsDialogHandler() {
    setTimeout(() => {
      // 'clickTypeIndicator' prevent single click from firing twice in case of double click
      if (this.clickTypeIndicator) {
        return true;
      } else {
        this.openCaseDetailsDialog.emit({ oldCaseCode: this.casesGanttData.oldCaseCode, pipelineId: this.casesGanttData.pipelineId });
      }
    }, 500);
    this.clickTypeIndicator = 0;
  }

  

  // ------------------------Mouse events-----------------------------------
  public initialWidth;
  projectMouseEnter(){
    let element: HTMLElement;
    const color = ProjectBackgroundColorCode[this.projectDetails.projectType] ?? ProjectBackgroundColorCode.ActiveCase;
    const borderClr = ProjectBorderColorCode[this.projectDetails.projectType] ?? ProjectBorderColorCode.ActiveCase;
    let border = '';

    if(this.ganttCase){
      element = this.ganttCase?.nativeElement;
      border = `1px solid ${borderClr}`;
    }else if(this.ganttPlanningCard){
      element = this.ganttPlanningCard?.nativeElement;
      border = `2px dotted ${borderClr}`;
    }
    
    this.initialWidth = element.offsetWidth;

    if (this.initialWidth < 350) {
      this.render.setStyle(element, "width", "fit-content");
      this.render.setStyle(element, "border", border);
      this.render.setStyle(element, "border-left", "none");
    }
    
    
      // if(this.initialWidth < 350){
      //   element.style.setProperty('width', 'fit-content');
      //   element.style.setProperty('background', color);
      //   element.style.setProperty('border', border);
      //   element.style.setProperty('border-left', 'none');
      // this.render.setStyle(
      //   element,
      //   'width',
      //   '450px'
      // );
      // this.render.setStyle(
      //   element,
      //   'background',
      //   color
      // );
      // this.render.setStyle(
      //   element,
      //   'border',
      //   border
      // );
      // this.render.setStyle(
      //   element,
      //   'border-left',
      //   'none'
      // );
      // }
    
  }

  projectMouseLeave(){
    let element: HTMLElement;

    if(this.ganttCase){
      element = this.ganttCase?.nativeElement;
    }else{
      element = this.ganttPlanningCard?.nativeElement;
    }

    this.render.setStyle(
      element,
      'width',
      this.initialWidth + 'px'
    );
    this.render.removeStyle(element, 'border-top');
    this.render.removeStyle(element, 'border-bottom');
    this.render.removeStyle(element, 'border-right');
  }

}
