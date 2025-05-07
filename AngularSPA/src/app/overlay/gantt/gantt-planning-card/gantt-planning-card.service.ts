/**
 * This file consists of logic associated with Gantt in case overlay
 */
import * as moment from 'moment';
import { CommitmentType } from './../../../shared/constants/enumMaster';
import { InvestmentCategory } from 'src/app/shared/interfaces/investmentCateogry.interface';
import { ResourceAllocation } from 'src/app/shared/interfaces/resourceAllocation.interface';
import { CommonService } from './../../../shared/commonService';
import { ValidationService } from 'src/app/shared/validationService';
import { DateService } from 'src/app/shared/dateService';
import { ResourceCommitment } from 'src/app/shared/interfaces/resourceCommitment';
interface GanttCaseObject {
  id?: string;
  parent?: string;
  oldCaseCode?: string;
  caseName: string;
  clientName?: string;
  caseTypeCode?: number;
  opportunityName?: string;
  planningCardId?: string;
  planningCardTitle?: string;
  employeeCode?: string;
  employeeName?: string;
  operatingOfficeCode?: number;
  operatingOfficeAbbreviation?: string;
  text: string;
  currentLevelGrade?: string;
  serviceLineCode?: string;
  serviceLineName?: string;
  commitmentType?: string;
  allocation?: number;
  start_date: Date;
  end_date: Date;
  pipelineId?: string;
  investmentCode?: number;
  caseRoleCode?: string;
  duration?: number;
  caseStartDate?: string;
  caseEndDate?: string;
  opportunityStartDate?: string;
  opportunityEndDate?: string;
  planningCardStartDate?: string;
  planningCardEndDate?: string;
  notes?: string;
  open?: boolean; // to make all the tasks expanded by default for tree-structure
  editable?: boolean;
  readonly?: boolean;
  render?: string;
  isChecked?: boolean;
  caseRoleName?: string;
  joiningDate?: string;
  isPlaceholderAllocation?: boolean;
  textColor: string;
  commitmentTypeCode: string;
  commitmentTypeName: string;
  positionGroupCode?: string;
}

export class GanttPlanningCardService {
  investmentCategories: InvestmentCategory[];
  ganttCaseInstance: any;
  contextMenuEvent: any;
  isGanttCalendarReadOnly: boolean;

  constructor(investmentCategories: InvestmentCategory[], ganttCaseInstance: any, isGanttCalendarReadOnly: boolean) {
    this.investmentCategories = investmentCategories;
    this.ganttCaseInstance = ganttCaseInstance;
    this.isGanttCalendarReadOnly = false;
  }

  public overrideGanttDefaultSettings() {

    this.displayTooltip();

    this.configureTextToTimelineBar();

    this.configureTextToTooltip();

    this.confgiureGanttTemplate();

    this.configureGridColumns();

    this.configureLayout();

    this.configureGanttBasicProperties();

    //readonly Gantt will have no mouse interactions
    if (!this.isGanttCalendarReadOnly) {

      this.attachClickEvents();
      this.configureListenerEvents();
      this.attachContextMenu();

    }

  }

  public sortGridColumns(allocations: ResourceAllocation[]): ResourceAllocation[] {
    return allocations.sort((a, b) => {
      // sometimes employee name is not present in workday
      if (a.employeeName && b.employeeName) {
        return (
          // sort placeholder(unnamed) and confirmed allocation first then by employee name and then by start date
          a.isPlaceholderAllocation === b.isPlaceholderAllocation) ? 0 : a.isPlaceholderAllocation ? -1 : 1
            || a.employeeName.localeCompare(b.employeeName)
            || <any>new Date(b.startDate) - <any>new Date(a.startDate);
      } else {
        return <any>new Date(b.startDate) - <any>new Date(a.startDate);
      }
    });
  }

  public setTodayLineInGanttChart() {
    const date_to_str = this.ganttCaseInstance.date.date_to_str(this.ganttCaseInstance.config.task_date);
    const markerId = this.ganttCaseInstance.addMarker({
      start_date: new Date(),
      css: 'today',
      text: 'Now',
      title: date_to_str(new Date())
    });
    this.ganttCaseInstance.getMarker(markerId);
  }

  public configureTimeLineColor() {
    this.ganttCaseInstance.templates.grid_row_class = function (start, end, task) {
      if (!task.editable) {
        return 'gainsboro';
      }
      // Group task row has $virtual property in-built
      if (task.$virtual)
        return "group-task"
    };

    this.ganttCaseInstance.templates.task_class = function (start, end, task) {
      if (task.investmentCode !== null && task.investmentCode !== undefined) {
        return 'grey';
      }
      if (task.isPlaceholderAllocation) {
        return 'placeholder-task-blue';
      }
      if (task.commitmentType !== undefined &&
        (task.commitmentType === CommitmentType.VACATION || task.commitmentType === CommitmentType.LOA || task.commitmentType === CommitmentType.TRAINING)) {
        return 'orange';
      }
      // Group task row has $virtual property in-built
      if (task.$virtual)
        return "group-task";
    };
  }

  public getInvestmentCategory(investmentCode) {
    return this.investmentCategories.find(investmentCategory => investmentCategory.investmentCode === investmentCode).investmentName;
  }

  public convertResourceAllocationsInGanttFormat(userAllocations: ResourceAllocation[]) {
    if ((!userAllocations || userAllocations.length === 0)) {
      return [];
    }

    return userAllocations.map(p => {
      const ganttCaseObject: GanttCaseObject = {
        id: p.id,
        oldCaseCode: p.oldCaseCode,
        caseName: p.caseName,
        clientName: p.clientName,
        caseTypeCode: p.caseTypeCode,
        opportunityName: p.opportunityName,
        planningCardId: p.planningCardId,
        planningCardTitle: p.planningCardTitle,
        employeeCode: p.employeeCode,
        employeeName: p.employeeName,
        operatingOfficeCode: p.operatingOfficeCode,
        operatingOfficeAbbreviation: p.operatingOfficeAbbreviation,
        text: (p.isPlaceholderAllocation && !p.employeeCode) ? 'Unnamed' : p.employeeName,
        currentLevelGrade: p.currentLevelGrade,
        serviceLineCode: p.serviceLineCode,
        serviceLineName: p.serviceLineName,
        allocation: p.allocation,
        start_date: new Date(new Date(p.startDate).setHours(0, 0, 0, 0)), // start_date and end_date are needed by gantt	        start_date: this.ganttCaseInstance.date.day_start(new Date(p.startDate.substring(0, p.startDate.indexOf('T')))), // start_date and end_date are needed by gantt
        end_date: new Date(new Date(p.endDate).setHours(23, 59, 59, 99)),
        pipelineId: p.pipelineId,
        investmentCode: p.investmentCode,
        caseRoleCode: p.caseRoleCode,
        duration: moment(p.endDate).diff(moment(p.startDate), 'days') + 1,
        caseStartDate: p.caseStartDate,
        caseEndDate: p.caseEndDate,
        opportunityStartDate: p.opportunityStartDate,
        opportunityEndDate: p.opportunityEndDate,
        notes: p.notes,
        open: true, // to make all the tasks expanded by default for tree-structure
        editable: true,
        isChecked: false,
        caseRoleName: p.caseRoleName,
        joiningDate: p.joiningDate,
        isPlaceholderAllocation: !!p.isPlaceholderAllocation,
        textColor: 'black',
        commitmentTypeCode: p.commitmentTypeCode,
        commitmentTypeName: p.commitmentTypeName,
        positionGroupCode: p.positionGroupCode
      };

      return ganttCaseObject;
    });
  }

  public convertResourcesCommitmentsInGanttFormat(allCommitments: any,
    userAllocation: ResourceAllocation[]) {

    const employeeCodes = this.getUniqueEmployeeCodesAsString(userAllocation);

    let convertedAllCommitmentsForResources = [];

    employeeCodes.split(',').map(employeeCode => {

      const allocation = userAllocation?.filter(ua => ua.employeeCode === employeeCode);

      if (allocation.length > 0) {

        const loas = this.getResourcesLoAs(employeeCode, allCommitments);
        const vacations = this.getResourcesVacations(employeeCode, allCommitments);
        const trainings = this.getResourcesTrainings(employeeCode, allCommitments);

        if (vacations?.length > 0)
          convertedAllCommitmentsForResources =
            convertedAllCommitmentsForResources.concat(this.addVacationAsChildItemToGanttAllocationRow(vacations, allocation));

        if (loas?.length > 0)
          convertedAllCommitmentsForResources =
            convertedAllCommitmentsForResources.concat(this.addLOAsAsChildItemToGanttAllocationRow(loas, allocation));

        if (trainings?.length > 0)
          convertedAllCommitmentsForResources =
            convertedAllCommitmentsForResources.concat(this.addTrainingsAsChildItemToGanttAllocationRow(trainings, allocation));
      }
    });

    return convertedAllCommitmentsForResources;
  }

  public updateHeaderCheckboxWhenScaleChanged() {
    if (this.ganttCaseInstance.getTaskCount() > 0) {
      let isAllChecked = true;
      let [isAllPlacholderAllocationsChecked, isAllConfirmedAllocationsChecked] = this.allAllocationChecked();
      this.ganttCaseInstance.eachTask(function (task) {
        if (task.parent === 0 && task.isChecked === false && task.editable === true) {
          isAllChecked = false;
        }
        if (task.$virtual && task.text === 'Placeholder') {
          task.isChecked = isAllPlacholderAllocationsChecked;
          this.updateTask(task.id);
        }
        if (task.$virtual && task.text === 'Confirmed') {
          task.isChecked = isAllConfirmedAllocationsChecked;
          this.updateTask(task.id);
        }
      });
      if ((document.getElementsByClassName('case-gantt-select-all')[0] as HTMLInputElement)) {
        (document.getElementsByClassName('case-gantt-select-all')[0] as HTMLInputElement).checked = isAllChecked;
      }
    }
  }

  public getResourcesLoAs(employeeCode, resourcesCommitments) {
    let loAs = resourcesCommitments.loAs?.filter(x => x.employeeCode === employeeCode)?.map(x => {
      return {
        type: x.type,
        startDate: DateService.convertDateInBainFormat(x.startDate),
        endDate: DateService.convertDateInBainFormat(x.endDate),
        description: x.description,
        employeeCode: x.employeeCode,
        source: 'Workday'
      };
    });

    const loASavedInBoss = resourcesCommitments.commitments
      ?.filter(x => x.employeeCode === employeeCode && x.commitmentTypeCode === CommitmentType.LOA)
      ?.map(x => {
        return {
          type: 'LOA',
          startDate: DateService.convertDateInBainFormat(x.startDate),
          endDate: DateService.convertDateInBainFormat(x.endDate),
          description: x.description,
          employeeCode: x.employeeCode,
          source: 'Staffing'
        };
      });

    loAs = loAs.concat(loASavedInBoss);
    loAs = loAs.sort((first, second) => {
      return <any>new Date(first.startDate) - <any>new Date(second.startDate);
    });

    return loAs;
  }

  public getResourcesVacations(employeeCode, resourcesCommitments: ResourceCommitment) {
    let employeeAllVacations = [];

    const vacationsInVRS = resourcesCommitments.vacations?.filter(x => x.employeeCode === employeeCode)?.map(x => {
      return {
        type: x.type,
        startDate: DateService.convertDateInBainFormat(x.startDate),
        endDate: DateService.convertDateInBainFormat(x.endDate),
        description: x.description,
        status: x.status,
        employeeCode: x.employeeCode,
        source: 'VRS',
        approvedBy: ''
      };
    });

    employeeAllVacations = employeeAllVacations.concat(vacationsInVRS);

    const vacationsSavedInBoss = resourcesCommitments.commitments
      ?.filter(x => x.employeeCode === employeeCode && x.commitmentTypeCode === CommitmentType.VACATION)
      ?.map(x => {
        return {
          type: 'Vacation',
          startDate: DateService.convertDateInBainFormat(x.startDate),
          endDate: DateService.convertDateInBainFormat(x.endDate),
          description: x.description,
          status: '',
          employeeCode: x.employeeCode,
          source: 'Staffing',
          approvedBy: ''
        };
      });

    employeeAllVacations = employeeAllVacations.concat(vacationsSavedInBoss);

    const vacationsSavedInWorkday = resourcesCommitments.timeOffs?.filter(x => x.employeeCode === employeeCode)?.map(x => {
      return {
        type: x.type,
        startDate: DateService.convertDateInBainFormat(x.startDate),
        endDate: DateService.convertDateInBainFormat(x.endDate),
        description: '',
        status: x.status,
        employeeCode: x.employeeCode,
        source: 'Workday',
        approvedBy: x.approvedBy
      };
    });

    employeeAllVacations = employeeAllVacations.concat(vacationsSavedInWorkday);

    employeeAllVacations = employeeAllVacations.sort((first, second) => {
      return <any>new Date(first.startDate) - <any>new Date(second.startDate);
    });

    return employeeAllVacations;
  }

  public getResourcesTrainings(employeeCode, resourcesCommitments: ResourceCommitment) {
    let employeeAllTrainings = [];
    const trainings = resourcesCommitments.trainings?.filter(x => x.employeeCode === employeeCode)?.map(x => {
      return {
        type: x.type,
        startDate: DateService.convertDateInBainFormat(x.startDate),
        endDate: DateService.convertDateInBainFormat(x.endDate),
        description: `${x.role} - ${x.trainingName}`,
        role: x.role,
        trainingName: x.trainingName,
        source: 'BVU'
      };
    });

    employeeAllTrainings = employeeAllTrainings.concat(trainings);

    const trainingsSavedInBoss = resourcesCommitments.commitments
      ?.filter(x => x.employeeCode === employeeCode && x.commitmentTypeCode === CommitmentType.TRAINING)
      ?.map(x => {
        return {
          type: 'Trainings',
          startDate: DateService.convertDateInBainFormat(x.startDate),
          endDate: DateService.convertDateInBainFormat(x.endDate),
          description: x.description,
          source: 'Staffing'
        };
      });

    employeeAllTrainings = employeeAllTrainings.concat(trainingsSavedInBoss);

    employeeAllTrainings = employeeAllTrainings.sort((first, second) => {
      return <any>new Date(first.startDate) - <any>new Date(second.startDate);
    });

    return employeeAllTrainings;
  }

  // private helper methods
  private allAllocationChecked() {
    let allPlaceholderSelected = true;
    let allConfirmedAllocationsSelected = true;
    this.ganttCaseInstance.eachTask(function (task) {
      if (task.isPlaceholderAllocation && task.isChecked === false && task.editable === true) {
        allPlaceholderSelected = false;
      }
      if (!task.isPlaceholderAllocation && task.isChecked === false && task.editable === true) {
        allConfirmedAllocationsSelected = false;
      }
    });

    return [allPlaceholderSelected, allConfirmedAllocationsSelected];

  }

  displayTooltip() {
    // Display tooltip only near timeline area
    this.ganttCaseInstance.attachEvent('onGanttReady', function () {
      const tooltips = this.ext.tooltips;
      tooltips.tooltip.setViewport(this.$task_data);
    });
  }

  attachClickEvents() {

    if (!this.ganttCaseInstance.checkEvent('onGridHeaderClick')) {
      this.ganttCaseInstance.attachEvent("onGridHeaderClick", function (name, e) {
        if (e.target.className === 'case-gantt-select-all') {
          this.eachTask(function (task) {
            task.isChecked = e.target.checked;
            this.updateTask(task.id);
            task.isChecked == true ? this.selectedAllocations.push(task) : this.selectedAllocations.splice(this.selectedAllocations.indexOf(task), 1);
          });
          this.selectedAllocations = this.selectedAllocations?.filter(c => c.parent === 0)?.filter(c => c.editable === true);
          return;
        } else {
          return true;
        }
      });
    }

    if (!this.ganttCaseInstance.checkEvent('onTaskClick')) {
      this.ganttCaseInstance.attachEvent('onTaskClick', function (id, e) {
        const checkbox = this.utils.dom.closest(e.target, '.gantt-case-checkbox-column');
        var task = this.getTask(id);
        // if checkbox is checked
        if (checkbox) {
          // Select all child elements for group selection
          if (task.$virtual) {
            if (task.text == 'Placeholder') {
              // Select All Placeholder Allocations
              this.eachTask(function (task) {
                if (task.isPlaceholderAllocation && task.text !== 'Confirmed') {
                  task.isChecked = e.target.checked;
                  this.updateTask(task.id);
                  task.isChecked == true ? this.selectedAllocations.push(task) : this.selectedAllocations.splice(this.selectedAllocations.indexOf(task), 1);
                }
              });
              this.selectedAllocations = this.selectedAllocations?.filter(c => !c.$virtual)?.filter(c => c.editable === true);
              return;
            } else if (task.text == 'Confirmed') {
              // Select All Confirmed Allocations
              this.eachTask(function (task) {
                if (!task.isPlaceholderAllocation && task.text !== 'Placeholder') {
                  task.isChecked = e.target.checked;
                  this.updateTask(task.id);
                  task.isChecked == true ? this.selectedAllocations.push(task) : this.selectedAllocations.splice(this.selectedAllocations.indexOf(task), 1);
                }
              });
              this.selectedAllocations = this.selectedAllocations?.filter(c => c => !c.$virtual)?.filter(c => c.editable === true);
              return;
            }
          }
          else {
            if (checkbox.checked) {
              this.selectedAllocations.push(task);
              task.isChecked = true;
              let isAllCommitmentsSelected = true;
              let isAllConfirmedAllocationsChecked = true;
              let isAllPlacholderAllocationsChecked = true;
              this.eachTask(function (task) {
                if (task.parent === 0 && task.isChecked === false && task.editable === true) {
                  isAllCommitmentsSelected = false;
                }
                if (!task.$virtual && task.isChecked === false && task.isPlaceholderAllocation) {
                  isAllPlacholderAllocationsChecked = false;
                }
                if (!task.$virtual && task.isChecked === false && !task.isPlaceholderAllocation) {
                  isAllConfirmedAllocationsChecked = false;
                }
              });

              this.eachTask(function (task) {
                if (isAllPlacholderAllocationsChecked && task.$virtual && task.text === 'Placeholder') {
                  task.isChecked = true;
                  this.updateTask(task.id);
                } else if (isAllConfirmedAllocationsChecked && task.$virtual && task.text === 'Confirmed') {
                  task.isChecked = true;
                  this.updateTask(task.id);
                }
              });
              if (isAllCommitmentsSelected) {
                (document.getElementsByClassName('case-gantt-select-all')[0] as HTMLInputElement).checked = true;
              }
            } else {
              this.eachTask(function (t) {
                if (task.isPlaceholderAllocation && t.$virtual && t.text === 'Placeholder') {
                  t.isChecked = false;
                  this.updateTask(t.id);
                } else if (!task.isPlaceholderAllocation && t.$virtual && t.text === 'Confirmed') {
                  t.isChecked = false;
                  this.updateTask(t.id);
                }

              });
              const allocationToDeleteIndex = this.selectedAllocations.find(sa => sa.id === id);
              this.selectedAllocations.splice(this.selectedAllocations.indexOf(allocationToDeleteIndex), 1);
              task.isChecked = false;
              (document.getElementsByClassName('case-gantt-select-all')[0] as HTMLInputElement).checked = false;
            }
          }
          return false;
        } else {
          // to remove the tooltip when click event fired
          if (document.getElementsByClassName('gantt_tooltip').length > 0) {
            document.getElementsByClassName('gantt_tooltip')[0].remove();
          }

          if (task.commitmentType === CommitmentType.VACATION) {
            return true;
          }

          if (e.target.className === 'task-title') {
            // the value of 'id' will be lost inside setTimeout function so using it here
            this.employeeCodeToOpen = this.getTask(id).employeeCode;
            // arrow function to retain the context of 'this' inside it
            setTimeout(() => {
              // 'clickTypeIndicator' prevent single click from firing twice in case of double click
              if (this.clickTypeIndicator) {
                return false;
              } else {
                this.openResourceOverlayPopup(this.employeeCodeToOpen);
                return true;
              }
            }, 300);
            this.clickTypeIndicator = 0;
          } else if (e.target.className === 'gantt_tree_content') {
            // to prevent gantt from re-rendering when clicked on non-editable column(s)
            // and that is making the checked checkboxes unchecked
            return false;
          } else if (task.commitmentType === CommitmentType.VACATION
            || task.commitmentType === CommitmentType.LOA ||
            task.commitmentType === CommitmentType.TRAINING) {
            return false;
          } else { return true; }
        }
      });
    }

    if (!this.ganttCaseInstance.checkEvent('onTaskDblClick')) {
      this.ganttCaseInstance.attachEvent('onTaskDblClick', function (id, e) {
        // to prevent double click event from firing while user check/uncheck the checkbox
        if (e.target.type === 'checkbox') {
          return false;
        }
        // to remove the tooltip when double click event fired
        if (document.getElementsByClassName('gantt_tooltip').length > 0) {
          document.getElementsByClassName('gantt_tooltip')[0].remove();
        }
        if (e.target.className.length > 0 && (e.target.className !== 'task-vacation' || e.target.className !== 'task-othercommitment')) {
          this.clickTypeIndicator = 1;
          const task = this.getTask(id);
          if (task.editable) {
            this.editResource(task);
          }
          return false; // prevent gantt's default pop-up to open
        }
      });
    }
  }

  configureTextToTimelineBar() {
    // Show Allocation on bar in the grid
    this.ganttCaseInstance.templates.task_text = (start, end, task) => {
      if (task.commitmentType !== undefined &&
        (task.commitmentType === CommitmentType.VACATION || task.commitmentType === CommitmentType.LOA || task.commitmentType === CommitmentType.TRAINING)) {
        return '';
      } else if (task.$virtual) { // Group task row has $virtual property in-built
        return `${task.text}`;
      } else {
        if (task.investmentCode !== null && task.investmentCode !== undefined) {
          const investmentCategory = this.getInvestmentCategory(task.investmentCode);
          if (investmentCategory.length > 0) {
            return `${task.text} - ${investmentCategory} (${task.allocation}%)`;
          }
        } else {
          return `${task.text} (${task.allocation}%)`;
        }
      }
    };
  }

  configureTextToTooltip(taskId = null) {
    this.ganttCaseInstance.templates.tooltip_text = (start, end, task) => {
      const allocation = task?.allocation?.toString().split('%')[0] + '%';
      if (taskId === task.id) {
        return '';
      }
      if (task.commitmentType !== undefined &&
        ( task.commitmentType === CommitmentType.LOA || task.commitmentType === CommitmentType.TRAINING)) {
        return `${task.text} (${moment(task.start_date).format('DD MMM YYYY')} - ${moment(task.end_date).format('DD MMM YYYY')})`;
      }
       else if(task.commitmentType === CommitmentType.VACATION){
        return `(${moment(task.start_date).format('DD MMM YYYY')} - ${moment(task.end_date).format('DD MMM YYYY')}), ${task.text}`;
       }
       else if (task.$virtual) { // Group task row has $virtual property in-built
        return '';
      } else {
        let allocationTooltipText = task.text + ' ' + allocation;
        const investmentCategory = !!task.investmentCode ? ' - ' + this.getInvestmentCategory(task.investmentCode) : '';
        const caseRoleNameText = !!task.caseRoleName ? ' - ' + task.caseRoleName : '';
        if (!!investmentCategory || !!caseRoleNameText) {
          allocationTooltipText += investmentCategory + caseRoleNameText;
        }
        return allocationTooltipText;
      }
    };

  }

  confgiureGanttTemplate() {
    this.ganttCaseInstance.templates.task_end_date = function (date) {
      return moment(new Date(date.valueOf() - 1)).format('DD-MMM-YYYY');
    };

    this.ganttCaseInstance.templates.grid_file = function (item) {
      return '';
    };

    this.ganttCaseInstance.templates.grid_folder = function () {
      return '';
    };

  }

  configureGridColumns() {
    const colNameWidth = this.isGanttCalendarReadOnly ? 170 : 130; //since select column will be hidden on readonly.

    // Columns will be added here to show on gantt chart. hide property will work only in PRO version
    this.ganttCaseInstance.config.columns = [
      {
        name: 'selectAll', hide: this.isGanttCalendarReadOnly, sort: false, label: '<input class="case-gantt-select-all" type="checkbox" name"selectAll" />', width: 30
      },
      {
        name: 'checked', hide: this.isGanttCalendarReadOnly, min_width: 0, max_width: 25, width: '*', template: function (task) {
          const checked = !!task.isChecked ? 'checked' : '';
          if (task.commitmentType !== undefined &&
            (task.commitmentType === CommitmentType.VACATION || task.commitmentType === CommitmentType.LOA || task.commitmentType === CommitmentType.TRAINING)) {
            return '';
          }
          return '<input class="gantt-case-checkbox-column" type="checkbox" name="test" id="chkbox_'
            + task.id + '" value="1" ' + checked + '/>';
        }
      },
      {
        name: 'text', tree: true, label: 'Name', width: colNameWidth, resize: true, template: function (task) {
          if (task.commitmentType !== undefined && task.commitmentType === CommitmentType.VACATION) {
            return '<span class="task-vacation">Vacation</span>';
          } else if (task.commitmentType === CommitmentType.LOA) {
            return '<span class="task-othercommitment">LOA</span>';
          } 
          else if (task.isPlaceholderAllocation && !task.employeeCode) {
            return '<span>' + task.text + '</span>';
          } 
          else if (task.commitmentType === CommitmentType.TRAINING) {
            return '<span class="task-othercommitment">Training</span>';
          } 
          else if (task.$virtual) { // Group task row has $virtual property in-built
            return '<span>' + task.text + '</span>';
          } else {
            return '<a href="javascript:void(0);" class="task-title">' + task.text + '</a>';
          }
        }
      },
      {
        name: 'serviceLineName', label: 'Service Line', width: 130, resize: true
      },
      {
        name: 'operatingOfficeAbbreviation', label: 'Office', align: 'center', width: 50, resize: true
      },
      { name: 'currentLevelGrade', label: 'LG', align: 'center', width: 50, resize: true },
      {
        name: 'start_date', width: 100, label: 'Start Date', template: function (task) {
          if (task.$virtual) { // Group task row has $virtual property in-built
            return '';
          } else {
            switch (task.commitmentType) {
              case CommitmentType.VACATION:
              case CommitmentType.LOA:
              case CommitmentType.TRAINING:
                return '';
              default:
                // tslint:disable-next-line: no-use-before-declare
                return validateDates(task, 'startDate');
            }
          }
        }, align: 'center', resize: true, editor: { type: 'date', map_to: 'start_date' }
      },
      {
        name: 'end_date', width: 130, label: 'End Date', template: function (task) {
          if (task.$virtual) { // Group task row has $virtual property in-built
            return '';
          } else {
            switch (task.commitmentType) {
              case CommitmentType.VACATION:
              case CommitmentType.LOA:
              case CommitmentType.TRAINING:
                return '';
              default:
                // tslint:disable-next-line: no-use-before-declare
                return validateDates(task, 'endDate');
            }
          }
        }, align: 'center', resize: true, editor: { type: 'date', map_to: 'end_date' }
      }
    ];
  }

  configureListenerEvents() {
    if (!this.ganttCaseInstance.ext.inlineEditors.checkEvent('onBeforeSave')) {
      this.ganttCaseInstance.ext.inlineEditors.attachEvent('onBeforeSave', function (state) {
        if (state.oldValue.toString() === state.newValue.toString()) {
          return false;
        }
        return true;
      });
    }

    if (!this.ganttCaseInstance.ext.inlineEditors.checkEvent('onEditStart')) {
      this.ganttCaseInstance.ext.inlineEditors.attachEvent('onEditStart', function (state) {
        const parentDiv = document.querySelectorAll('input[type=date]')[0].parentElement;
        parentDiv.style.width = '100px';
      });
    }

    //this event is called on task drag, resize etc
    // if (!this.ganttCaseInstance.checkEvent('onBeforeTaskChanged')) {
    //   this.ganttCaseInstance.attachEvent('onBeforeTaskChanged', function (id, mode, task) {
    //     const _self = this;
    //     const updatedTask = _self.getTask(id);
    //     const existingAllocation = _self.getAllocationDataFromGanttTask([].concat(task));
    //     const updatedAllocation = _self.getAllocationDataFromGanttTask([].concat(updatedTask));

    //     const data1 = _self.componentInstance.validateMonthCloseForUpdates(updatedAllocation, existingAllocation);
    //     if(!data1[0])
    //     _self.componentInstance.notifyService.showWarning(data1[1]);
    //     return data1[0];
    //   });
    // }

  }

  configureLayout() {
    // Separate Horizontal bar for gantt chart grid and tabular grid
    this.ganttCaseInstance.config.layout = {
      css: 'gantt_container',
      cols: [
        {
          width: 630,
          min_width: 300,
          rows: [
            { view: 'grid', scrollX: 'gridScroll', scrollable: true, scrollY: 'scrollVer' },
            { view: 'scrollbar', id: 'gridScroll', group: 'horizontal' }
          ]
        },
        { resizer: true, width: 1 },
        {
          rows: [
            { view: 'timeline', scrollX: 'scrollHor', scrollY: 'scrollVer' },
            { view: 'scrollbar', id: 'scrollHor', group: 'horizontal' }
          ]
        },
        { view: 'scrollbar', id: 'scrollVer' }
      ]
    };
  }

  setGanttReadonly() {
    this.ganttCaseInstance.config.readonly = true;
    this.ganttCaseInstance.config.editable_property = "readonly"; //changing from editable column to readonly column on ganttCaseObject to make sure evry row is non-editable
  }

  configureGanttBasicProperties() {
    this.ganttCaseInstance.config.sort = true;
    this.ganttCaseInstance.config.readonly = false;
    this.ganttCaseInstance.config.date_grid = '%d-%M-%Y'; // show dates in bain standard dd-MMM-YYYY format
    this.ganttCaseInstance.config.drag_links = false; // prevent linking of items in gantt
    this.ganttCaseInstance.config.inline_editors_date_processing = 'keepDates';
    this.ganttCaseInstance.config.round_dnd_dates = false;

    if (this.isGanttCalendarReadOnly) {
      this.setGanttReadonly();
    }

  }

  configureTaskGrouping(havePlaceholderAllocation) {
    this.ganttCaseInstance.serverList("AllocationType",
      [
        { key: true, label: "Placeholder" },
        { key: false, label: "Confirmed" }
      ]);
    if (havePlaceholderAllocation) {
      this.ganttCaseInstance.groupBy({
        groups: this.ganttCaseInstance.serverList("AllocationType"),
        relation_property: "isPlaceholderAllocation",
        group_id: "key",
        group_text: "label"
      });
    } else {
      this.ganttCaseInstance.groupBy(false);
    }
  }

  getUniqueEmployeeCodesAsString(resourceArray): string {
    const distinctEmployeeCodes = resourceArray.map(e => e.employeeCode);
    const employeeCodes = distinctEmployeeCodes?.filter((item, i, ar) => ar.indexOf(item) === i).join(',');
    return employeeCodes;
  }

  getVacationFormattedData(eg) {
    let data ="";
    if(!ValidationService.isStringNullOrEmpty(eg.approvedBy) && !ValidationService.isStringNullOrEmpty(eg.status) && eg.status == 'Approved'){
    data = `Approved By: ${eg.approvedBy}`;
    }
    else if(!ValidationService.isStringNullOrEmpty(eg.status) && eg.status === 'Submitted'){
    data = "Pending Approval";
    }
    else if(!ValidationService.isStringNullOrEmpty(eg.status) && eg.status ){
      data = eg.status;
    }

    if(!ValidationService.isStringNullOrEmpty(eg.description)){
      data += `- ${eg.description}`;
    }
    
    if(ValidationService.isStringNullOrEmpty(data)){
      data = "Vacation";
    }
    return data;
}

  addVacationAsChildItemToGanttAllocationRow(employeeVacations, allocation) {

    const uniqueVacationId = CommonService.generate_UUID();

    let ganttResourceVacations = [];
    ganttResourceVacations = ganttResourceVacations.concat(employeeVacations.map(eg => {
      var data = this.getVacationFormattedData(eg);
      return {
        parent: uniqueVacationId,
        caseName: eg.status ? `${eg.status} - ${eg.description}` : `${eg.description}`,
        employeeCode: eg.employeeCode,
        text: data,
        start_date: new Date(new Date(eg.startDate).setHours(0, 0, 0, 0)),
        end_date: new Date(new Date(eg.endDate).setHours(23, 59, 59, 99)),
        commitmentType: CommitmentType.VACATION,
        readonly: true
      };
    }));

    ganttResourceVacations = ganttResourceVacations.concat({
      id: uniqueVacationId,
      parent: allocation[0].id,
      caseName: 'Vacation',
      text: 'Vacation',
      start_date: new Date(new Date(employeeVacations[0].startDate).setHours(0, 0, 0, 0)),
      end_date: new Date(new Date(employeeVacations[employeeVacations.length - 1].endDate).setHours(23, 59, 59, 99)),
      commitmentType: CommitmentType.VACATION,
      readonly: true,
      render: 'split'
    });

    return ganttResourceVacations;
  }

  addLOAsAsChildItemToGanttAllocationRow(employeeLoa, allocation) {
    const ganttresourceLoas = employeeLoa.map(eg => {

      return {
        parent: allocation[0].id,
        caseName: `${eg.description}`,
        employeeCode: eg.employeeCode,
        text: eg.description || 'LOA',
        start_date: new Date(new Date(eg.startDate).setHours(0, 0, 0, 0)),
        end_date: new Date(new Date(eg.endDate).setHours(23, 59, 59, 99)),
        commitmentType: CommitmentType.LOA,
        readonly: true
      };

    });

    return ganttresourceLoas;
  }

  addTrainingsAsChildItemToGanttAllocationRow(employeeTrainings, allocation) {
    const ganttresourceTrainings = employeeTrainings.map(eg => {

      return {
        parent: allocation[0].id,
        caseName: `${eg.description}`,
        employeeCode: eg.employeeCode,
        text: eg.description || 'Training',
        start_date: new Date(new Date(eg.startDate).setHours(0, 0, 0, 0)),
        end_date: new Date(new Date(eg.endDate).setHours(23, 59, 59, 99)),
        commitmentType: CommitmentType.TRAINING,
        readonly: true
      };

    });

    return ganttresourceTrainings;
  }

  private attachContextMenu() {
    document.addEventListener('mousedown', () => {
      if (document.getElementById('ganttCaseContextMenuId')) {
        document.getElementById('ganttCaseContextMenuId').innerHTML = '';
        this.configureTextToTooltip(null);
      }
    });

    document.addEventListener('wheel', () => {
      if (document.getElementById('ganttCaseContextMenuId')) {
        document.getElementById('ganttCaseContextMenuId').innerHTML = '';
      }
    });

    if (document.getElementById('ganttCaseContextMenuId')) {
      document.getElementById('ganttCaseContextMenuId').addEventListener('mousedown', () => {
        event.stopPropagation();
      });
    }
    let contextMenuTaskId = null;
    this.ganttCaseInstance.attachEvent('onMouseMove', () => {
      if (document.getElementById('ganttCaseContextMenuId')) {
        if (document.getElementById('ganttCaseContextMenuId').innerHTML !== '') {
          this.configureTextToTooltip(contextMenuTaskId);
          return;
        }
      }
      this.configureTextToTooltip(null);
    });
    this.ganttCaseInstance.attachEvent('onContextMenu', function (taskId, linkId, event) {
      if (document.getElementsByClassName('gantt_tooltip').length > 0) {
        document.getElementsByClassName('gantt_tooltip')[0].remove();
      }

      if (taskId) {
        // Do not show context menu for commitments(vacation only) and group header
        if (this.getTask(taskId)?.commitmentType !== 'V' && !this.getTask(taskId)?.$virtual) {
          const ganttChartHeight = (document.getElementsByClassName('case-gantt-chart')[0] as HTMLElement).offsetHeight;
          contextMenuTaskId = taskId;
          const x = event.clientX + document.body.scrollLeft + document.documentElement.scrollLeft;
          const y = (event.clientY > (ganttChartHeight - 60) ? '80%' : event.clientY + 'px');
          document.getElementById('ganttCaseContextMenuId').innerHTML =
            '<ul id=\'gantt-case-context-menu-ul\' style=\'top:' + y + ' !important;left:' + x + 'px !important\'></ul>';
          const allocations = this.selectedAllocations.length < 1 ? [this.getTask(taskId)] : this.selectedAllocations;
          // split allocation menu option definition
          if (!this.getTask(taskId)?.isPlaceholderAllocation) {
            this.addContextMenuItem('Split Allocation', allocations, 'Split');
          }

          // delete menu option definition
          this.addContextMenuItem('Delete', allocations, 'Delete');

          // update start date menu option definition
          this.addContextMenuItem('Update Start Date', allocations, 'Start');

          // update end date menu option definition
          this.addContextMenuItem('Update End Date', allocations, 'End');
        }
      }
    });
  }
}

// START: clousres definition for gantt's template functions
function allocationDatesValidator() {
  return function (task, dateType) {
    // because wrapping a date into a 'div' will break gantt's default date format,
    // we need to convert it on our own
    const intendedDate = dateType === 'startDate' ? task.start_date : task.end_date;
    const intendedMonth = new Date(intendedDate);
    const month = intendedMonth.toLocaleString('default', { month: 'short' });
    const year = intendedDate.getFullYear();
    const day = intendedDate.getDate().toString().padStart(2, '0');
    const bainFormattedEndDate = day + '-' + month + '-' + year;
    if (task.start_date > task.end_date ||
      (dateType === 'startDate' && new Date(task.joiningDate) > task.start_date)) {
      return '<div style="background-color: lightcoral;width: inherit; height: inherit" tooltip="test">' +
        bainFormattedEndDate + '</div>';
    } else {
      return '<div id="' + task.id + '_startDate">' + bainFormattedEndDate + '</div>';
    }
  };
}
const validateDates = allocationDatesValidator();
// END
