import { Component, ElementRef, EventEmitter, Input, OnInit, Output, Renderer2, SimpleChanges, ViewChild } from '@angular/core';
import * as moment from 'moment';
import { CaseRollDialogService } from 'src/app/overlay/dialogHelperService/caseRollDialog.service';
import { OverlayDialogService } from 'src/app/overlay/dialogHelperService/overlayDialog.service';
import { ShowQuickPeekDialogService } from 'src/app/overlay/dialogHelperService/show-quick-peek-dialog.service';
import { ConstantsMaster } from 'src/app/shared/constants/constantsMaster';
import { ProjectBackgroundColorCode, ProjectBorderColorCode, ProjectType } from 'src/app/shared/constants/enumMaster';
import { DateService } from 'src/app/shared/dateService';
import { PlanningCard } from 'src/app/shared/interfaces/planningCard.interface';
import { Project } from 'src/app/shared/interfaces/project.interface';
import { NotificationService } from 'src/app/shared/notification.service';
import { ValidationService } from 'src/app/shared/validationService';
import { ResourcesCommitmentsDialogService } from 'src/app/overlay/dialogHelperService/resourcesCommitmentsDialog.service';

@Component({
  selector: 'app-gantt-project-details',
  templateUrl: './gantt-project-details.component.html',
  styleUrls: ['./gantt-project-details.component.scss']
})
export class GanttProjectDetailsComponent implements OnInit {

  constructor(
    private notifyService: NotificationService,
    private overlayDialogService: OverlayDialogService,
    private showQuickPeekDialogService: ShowQuickPeekDialogService,
    private caseRollDialogService: CaseRollDialogService,
    private render: Renderer2,
    private resourcesCommitmentsDialogService: ResourcesCommitmentsDialogService) { }

  // ---------------- Input Events -------------------- //
  @Input() casesGanttData: Project;
  @Input() planningCard: PlanningCard;
  @Input() project: any;
  @Input() dateRange: [Date, Date];

  @Input() bounds: HTMLElement;
  @Input() caseIndex: number;

  // -----------------Output Events ------------------- //
  @Output() openPlaceholderForm = new EventEmitter<any>();
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
  projectDetails = { 
    planningCardId: "",
    projectName: "", 
    caseName: "",
    oldCaseCode: "",
    pipelineId: "",
    projectType: "", 
    startDate: "", 
    endDate: "", 
    officeAbbreviation: "", 
    isCaseRoll: false, 
    placeholderAllocations: [],
    allocations: [],
    skuTerm : '' }
   
  activeResourcesEmailAddresses:string = "";
  accessibleFeatures = ConstantsMaster.appScreens.feature;


  // ------------------------Life Cycle Events---------------------------------------

  ngOnInit() {
    this.setProjectDetails();
    this.setCommitmentClasses();
    this.getActiveResourcesEmailAddress();
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes.project && this.project) {
      this.setProjectDetails();
      this.setCommitmentClasses();
      this.getActiveResourcesEmailAddress();
    }
  }

  private setProjectDetails() {
    if (this.casesGanttData) {
      this.setProjectTitle(this.casesGanttData);
      this.projectDetails.projectType = this.casesGanttData.type;
      this.projectDetails.caseName = this.casesGanttData.caseName ?? this.casesGanttData.opportunityName ?? "";
      this.projectDetails.oldCaseCode = this.casesGanttData.oldCaseCode;
      this.projectDetails.pipelineId = this.casesGanttData.pipelineId;
      this.projectDetails.startDate = this.casesGanttData.startDate;
      this.projectDetails.endDate = this.casesGanttData.endDate;
      this.projectDetails.officeAbbreviation = this.casesGanttData.managingOfficeAbbreviation;
      this.projectDetails.isCaseRoll = this.casesGanttData.caseRoll ? true : false;
      this.projectDetails.skuTerm = this.casesGanttData.skuCaseTerms?.skuTerms?.map(s => s.name).toString() ?? '';
      this.projectDetails.placeholderAllocations = this.casesGanttData?.placeholderAllocations;
    }
    else if (this.planningCard) {
      this.projectDetails.planningCardId = this.planningCard.id || "";
      this.projectDetails.projectType = ProjectType.PlanningCard;
      this.projectDetails.projectName = this.planningCard.name;
      this.projectDetails.caseName = this.planningCard.name || "";
      this.projectDetails.startDate = DateService.convertDateInBainFormat(this.planningCard.startDate);
      this.projectDetails.endDate = DateService.convertDateInBainFormat(this.planningCard.endDate);
      this.projectDetails.placeholderAllocations = this.planningCard?.allocations.filter((allocation) => 
        allocation.isPlaceholderAllocation || allocation.isConfirmed);

    }
    else if (this.project) {
      this.projectDetails.planningCardId = this.project.planningCardId || "";
      this.projectDetails.projectType = this.project.type;
      this.project.planningCardId ? this.projectDetails.projectName = this.project.caseName : this.setProjectTitle(this.project);
      this.projectDetails.caseName = this.project.caseName;
      this.projectDetails.oldCaseCode = this.project.caseCode;
      this.projectDetails.pipelineId = this.project.pipelineId;
      this.projectDetails.startDate = DateService.convertDateInBainFormat(this.project.startDate);
      this.projectDetails.endDate = DateService.convertDateInBainFormat(this.project.endDate);
      this.projectDetails.officeAbbreviation = this.project.managingOfficeAbbreviation;
      this.projectDetails.isCaseRoll = this.project.caseRoll ? true : false;
      this.projectDetails.skuTerm = this.project.skuCaseTerms?.skuTerms?.map(s => s.name).toString() ?? '';
      this.projectDetails.placeholderAllocations = this.project?.placeholderAllocations;
    }
  }

  // ------------------------Helper Functions-----------------------------------

  private setProjectTitle(project) {

    switch (project.type) {
      case ProjectType.Opportunity: {
        if (project.probabilityPercent) {
          this.projectDetails.projectName = `${project.probabilityPercent}% - ${project.clientName} - ${project.opportunityName || project.caseName}`; // TODO: Need to check if this is correct
        } else {
          this.projectDetails.projectName = `${project.clientName} - ${project.opportunityName || project.caseName}`;
        }
        break;
      }
      case ProjectType.PlanningCard: {
        this.projectDetails.projectName = `${project.name || project.caseName}`;
        break;
      }
      default: {
        this.projectDetails.projectName = `${project.oldCaseCode || project.caseCode} - ${project.clientName} - ${project.caseName}`;
        break;
      }
    }
  }

  private setCommitmentClasses(reAdjustClass = false, $event = null) {

    let startCount = 0;
    const commitmentStartDate = this.casesGanttData
      ? moment(this.casesGanttData.startDate).startOf('day')
      : (this.planningCard ? moment(this.planningCard.startDate).startOf('day')
        : moment(this.project.startDate).startOf('day'));
    const dateRangeStartDate = moment(this.dateRange[0]).startOf('day');
    const commitmentEndDate = this.casesGanttData
      ? moment(this.casesGanttData.endDate).startOf('day')
      : (this.planningCard ? moment(this.planningCard.endDate).startOf('day')
        : moment(this.project.endDate).startOf('day'));
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
      this.className = 'start-' + startCount + ' duration-' + duration + ' commitment-' + this.getCommitmentColor();
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

      case ProjectType.PlanningCard:
        if(this.planningCard?.includeInCapacityReporting || this.project?.includeInCapacityReporting) {
          color = 'red';
        } else {
          color = 'green';
        }
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
    } else if (this.project?.allocatedResources) {
      this.project.allocatedResources.forEach(resource => {
        if (!this.activeResourcesEmailAddresses.includes(resource.internetAddress)) {
          this.activeResourcesEmailAddresses += resource.internetAddress + ';';
        }
      });
    }
  }

  onCaseRollHandler() {
    if(this.casesGanttData) {
      if (!ValidationService.isCaseEligibleForRoll(this.casesGanttData.endDate)) {
        this.notifyService.showValidationMsg(ValidationService.caseRollNotAllowedForInActiveCasesMessage);
      } else {
        this.caseRollDialogService.openCaseRollFormHandler({ project: this.casesGanttData });
      }
    } else if(this.project) {
      if (!ValidationService.isCaseEligibleForRoll(this.project.endDate)) {
        this.notifyService.showValidationMsg(ValidationService.caseRollNotAllowedForInActiveCasesMessage);
      } else {
        this.caseRollDialogService.openCaseRollFormHandler({ project: this.project });
      }
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
    } else if(this.project) {
      employees = this.project.allocatedResources?.map(x => {
        return {
          employeeCode: x.employeeCode,
          employeeName: x.employeeName,
          levelGrade: x.currentLevelGrade
        };
      });
    }

    this.showQuickPeekDialogHandler(employees);
  }

  showQuickPeekDialogHandler(event) {
    this.resourcesCommitmentsDialogService.showResourcesCommitmentsDialogHandler(event);

  }

  addPlaceholder(projectToOpen) {
    this.openAddTeamSkuForm.emit(projectToOpen);
  }

  public openDetailsDialogHandler() {
    setTimeout(() => {
      // 'clickTypeIndicator' prevent single click from firing twice in case of double click
      if (this.clickTypeIndicator) {
        return true;
      } else {
        if (this.project) {
          if (this.project.planningCardId) {
            this.overlayDialogService.openPlanningCardDetailsDialogHandler(this.project.planningCardId);
          } else {
            this.overlayDialogService.openProjectDetailsDialogHandler({ oldCaseCode: this.project.caseCode, pipelineId: this.project.pipelineId });
          }
        } else if(this.planningCard) {
          this.overlayDialogService.openPlanningCardDetailsDialogHandler(this.planningCard.id);   
        } else {
          this.overlayDialogService.openProjectDetailsDialogHandler({ oldCaseCode: this.casesGanttData.oldCaseCode, pipelineId: this.casesGanttData.pipelineId });
        }}
    }, 500);
    this.clickTypeIndicator = 0;
  }



  //------------------------Mouse events-----------------------------------
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
