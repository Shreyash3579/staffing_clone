/**
 * This file consists of logic associated with Gantt in resource overlay
 */

import * as moment from 'moment';
import { CoreService } from 'src/app/core/core.service';
import { CommonService } from 'src/app/shared/commonService';
import { InvestmentCategory } from 'src/app/shared/interfaces/investmentCateogry.interface';
import {
  CommitmentType as CommitmentTypeEnum,
  EmployeeJobChangeStatus as EmployeeJobChangeStatusEnum,
  GanttTimelineColor as GanttTimelineColorEnum,
  ProjectType as ProjectTypeEnum,
  TaskType as TaskTypeEnum
} from '../../../shared/constants/enumMaster';

interface GanttResourceObject {
  id: string;
  parent: string;
  caseName: string;
  clientName: string;
  levelGrade: string;
  oldCaseCode: string;
  pipelineId: string;
  opportunityName: string;
  operatingOfficeCode: number;
  operatingOfficeAbbreviation: string;
  operatingOfficeName: string;
  employeeCode: string;
  employeeName: string;
  serviceLineCode: string;
  serviceLineName: string;
  text: string;
  start_date: Date;
  end_date: Date;
  allocation: number;
  description: string;
  type: string;
  commitmentTypeCode: string;
  commitmentTypePrecedence: string;
  investmentCode: number;
  caseRoleCode: string;
  duration: number;
  editable: string;
  render: string;
  caseStartDate: Date;
  caseEndDate: Date;
  opportunityStartDate: Date;
  opportunityEndDate: Date;
  notes: string;
  isChecked?: boolean;
  caseRoleName?: string;
  joiningDate?: Date;
  textColor?: string;
  placeholderAllocation: boolean;
}
export class GanttResourceService {
  ganttInstance: any;
  investmentCategories: InvestmentCategory[];
  coreService: CoreService;

  constructor(ganttInstance: any, investmentCategories: InvestmentCategory[], _coreService: CoreService) {
    this.ganttInstance = ganttInstance;
    this.investmentCategories = investmentCategories;
    this.coreService = _coreService;
  }

  public overrideGanttDefaultSettings() {
    this.displayTooltip();

    this.attachClickEvents();

    this.configureTextToTimelineBar();

    this.configureTextToTooltip();

    this.configureGridColumns();

    this.configureLayout();

    this.configureListenerEvents();

    this.configureGanttBasicProperties();
    this.attachContextMenu();
  }

  public changeColorOrShapeOfTaskByCommitmentType() {
    this.ganttInstance.templates.grid_row_class = function (start, end, task) {
      
      if (!task.editable) {
        return task.ganttTimelineColorEnum.GAINS_BORO;
        //return 'gainsboro';
      }
      // Group task row has $virtual property in-built
      if (!task.commitmentTypeCode)
        return "group-task";
    };

    this.ganttInstance.templates.task_class = function (_start, _end, task) {
      if (!task.commitmentTypeCode)
        return "hidden-task";
      else 
        return getTimelineColor(task);
      
    }
  }

  public getCommitmentsForGantt(resourceCommitments, resource) {
    if (!resourceCommitments || resourceCommitments.length === 0) {
      return [];
    }
    let index = 0;
    let editableCommitments: string[] = CommonService.getEditableCommitmentTypesCodesList(this.coreService.loggedInUserClaims);

    const ganttResourceObject: GanttResourceObject[] = resourceCommitments.map(r => {
      index++;
      return {
        id: r.id ? r.id : index,
        parent: r.parent,
        caseName: r.caseName,
        caseTypeCode: r.caseTypeCode,
        clientName: r.clientName,
        levelGrade: r.levelGrade,
        oldCaseCode: r.oldCaseCode,
        pipelineId: r.pipelineId,
        opportunityName: r.opportunityName,
        operatingOfficeCode: r.operatingOfficeCode ?? resource?.schedulingOffice?.officeCode,
        operatingOfficeAbbreviation: r.operatingOfficeAbbreviation ?? resource?.schedulingOffice?.officeAbbreviation,
        operatingOfficeName: r.operatingOfficeName ?? resource?.office?.officeName,
        employeeCode: r.employeeCode ?? resource?.employeeCode,
        employeeName: r.employeeName ?? resource?.fullName,
        serviceLineCode: r.serviceLineCode ?? resource?.serviceLine?.serviceLineCode,
        serviceLineName: r.serviceLineName ?? resource?.serviceLine?.serviceLineName,
        text: this.getTextFromDescription(r),
        start_date: new Date(new Date(r.startDate).setHours(0, 0, 0, 0)), // start_date and end_date are needed by gantt	        start_date: this.ganttInstance.date.day_start(new Date(r.startDate.substring(0, r.startDate.indexOf('T')))), // start_date and end_date are needed by gantt
        end_date: new Date(new Date(r.endDate).setHours(23, 59, 59, 99)),
        allocation: r.allocation == null ? '100%' : r.allocation,
        description: r.description,
        type: r.type,
        commitmentTypeCode: this.getCommitmentTypeCode(r),
        commitmentTypePrecedence: r.commitmentTypePrecedence,
        commitmentTypeReasonCode: r.commitmentTypeReasonCode,
        investmentCode: r.investmentCode,
        caseRoleCode: r.caseRoleCode,
        duration: moment(r.endDate).diff(moment(r.startDate), 'days') + 1,
        editable: !r.placeholderAllocation && this.isCommitmentEditable(editableCommitments, r), //placeholderAllocations are non-editable for now
        render: r.render,
        caseStartDate: r.caseStartDate,
        caseEndDate: r.caseEndDate,
        opportunityStartDate: r.opportunityStartDate,
        opportunityEndDate: r.opportunityEndDate,
        notes: r.notes,
        isChecked: false,
        caseRoleName: r.caseRoleName,
        isPlaceholderAllocation: r.isPlaceholderAllocation ?? false,
        //isPlaceholderAllocation: r.isPlaceholderAllocation? true : (r.planningCardId? true : false),   
        planningCardId: r.planningCardId,
        commitmentTypeName: r.commitmentTypeName,
        commitmentTypeEnum: CommitmentTypeEnum,
        employeeJobChangeStatusEnum: EmployeeJobChangeStatusEnum,
        ganttTimelineColorEnum: GanttTimelineColorEnum,
        taskTypeEnum: TaskTypeEnum,
        joiningDate: r.joiningDate ?? resource?.startDate,
        textColor: "Black",
        open: true
      };
    });
    return ganttResourceObject;
  }

  private getCommitmentTypeCode(commitment) {
    // return (commitment.type === 'Case' || commitment.type === 'Opportunity') ? 'C' : commitment.commitmentTypeCode;
    return commitment.commitmentTypeCode;
  }

  private isCommitmentEditable(editableCommitments, commitment) {
    return commitment.source === 'staffing' && editableCommitments.some(x => x === this.getCommitmentTypeCode(commitment));
  }

  // private helper methods
  private displayTooltip() {
    // Display tooltip only near timeline area
    this.ganttInstance.attachEvent('onGanttReady', function () {
      const tooltips = this.ext.tooltips;
      tooltips.tooltip.setViewport(this.$task_data);
    });
  }

  private attachClickEvents() {
    if (!this.ganttInstance.checkEvent('onGridHeaderClick')) {
      this.ganttInstance.attachEvent("onGridHeaderClick", function (name, e) {
        if (e.target.className === 'resource-gantt-select-all') {
          this.eachTask(function (task) {
            task.isChecked = e.target.checked;
            this.updateTask(task.id);
            task.isChecked == true ? this.selectedAllocations.push(task) : this.selectedAllocations.splice(this.selectedAllocations.indexOf(task), 1);
          });
          this.selectedAllocations = this.selectedAllocations?.filter(c => c.parent !== 0)?.filter(c => c.editable === true);
          return;
        } else {
          return true;
        }
      });
    }


    if (!this.ganttInstance.checkEvent('onTaskClick')) {
      this.ganttInstance.attachEvent('onTaskClick', function (id, e) {
        const checkbox = this.utils.dom.closest(e.target, '.resource-gantt-checkbox-column');
        // if checkbox is checked
        if (checkbox) {
          if (checkbox.checked) {
            this.selectedAllocations.push(this.getTask(id));
            this.getTask(id).isChecked = true;
            let isAllCommitmentsSelected = true;
            this.eachTask(function (task) {
              if (task.parent !== 0 && task.isChecked === false && task.editable === true) {
                isAllCommitmentsSelected = false;
              }
            });
            if (isAllCommitmentsSelected) {
              (document.getElementsByClassName('resource-gantt-select-all')[0] as HTMLInputElement).checked = true;
            }
          } else {
            const allocationToDeleteIndex = this.selectedAllocations.find(sa => sa.id === id);
            this.selectedAllocations.splice(this.selectedAllocations.indexOf(allocationToDeleteIndex), 1);
            this.getTask(id).isChecked = false;
            (document.getElementsByClassName('resource-gantt-select-all')[0] as HTMLInputElement).checked = false;
          }
          return false;
        } else {
          // to remove the tooltip when double click event fired
          if (document.getElementsByClassName('gantt_tooltip').length > 0) {
            document.getElementsByClassName('gantt_tooltip')[0].remove();
          }
          switch (this.getTask(id).type) {
            case 'Holiday':
            case 'Vacation':
              return true;
          }
          if (e.target.className.indexOf('task-title') > -1) {
            // the value of 'id' will be lost inside setTimeout function so using it here
            this.oldCaseCode = this.getTask(id).oldCaseCode;
            this.pipelineId = this.getTask(id).pipelineId;
            this.isTaskCaseType = this.getTask(id).commitmentTypeCode === 'C';
            // arrow function to retain the context of 'this' inside it
            setTimeout(() => {
              // 'clickTypeIndicator' prevent single click from firing twice in case of double click
              if (this.clickTypeIndicator) {
                return false;
              } else {
                if (this.isTaskCaseType) {
                  this.openCaseOverlayPopup(this.oldCaseCode, this.pipelineId);
                  return true;
                }
              }
            }, 300);
            this.clickTypeIndicator = 0;
          }
        }
      });
    }

    if (!this.ganttInstance.checkEvent('onTaskDblClick')) {
      this.ganttInstance.attachEvent('onTaskDblClick', function (id, e) {
        // to prevent double click event from firing while user check/uncheck the checkbox
        if (e.target.type === 'checkbox') {
          return false;
        }
        // to remove the tooltip when double click event fired
        if (document.getElementsByClassName('gantt_tooltip').length > 0) {
          document.getElementsByClassName('gantt_tooltip')[0].remove();
        }
        this.clickTypeIndicator = 1;
        const task = this.getTask(id);
        if (task.editable) {
          this.editResourceCommitment(task);
        }
        return false; // prevent gantt's default pop-up to open
      });
    }
  }

  private configureTextToTimelineBar() {
    this.ganttInstance.templates.task_text = (start, end, task) => {
      const allocation = task?.allocation?.toString().split('%')[0] + '%';
      if (task.type === task.taskTypeEnum.HOLIDAY || task.type === task.taskTypeEnum.VACATION
        || task.type === task.taskTypeEnum.TRANSFER) {
        return '';
      } 
      else if (!task.commitmentTypeCode) { // dummy parent rows
        return '';
      }
      else {
        if (task.investmentCode !== null && task.investmentCode !== undefined) {
          const investmentCategory = this.getInvestmentCategory(task.investmentCode);
          if (investmentCategory?.length > 0) {
            return `${task.text} - ${investmentCategory} (${allocation})`;
          }
        } else {
          return `${task.text} (${allocation})`;
        }
      }
    };
  }

  private getInvestmentCategory(investmentCode) {
    return this.investmentCategories.find(investmentCategory => investmentCategory.investmentCode === investmentCode)?.investmentName;
  }

  private configureTextToTooltip(taskId = null) {
    this.ganttInstance.templates.tooltip_text = (start, end, task) => {
      const allocation = task?.allocation?.toString().split('%')[0] + '%';
      const formattedStartDate = moment(task?.start_date).format('DD MMM YYYY');
      const formattedEndDate = moment(task?.end_date).format('DD MMM YYYY');
      if (taskId === task.id) {
        return '';
      }
      if (!task.commitmentTypeCode) { // dummy aprent row
        return '';
      }
      switch (task.type) {
        //case undefined || CommitmentTypeEnum.HOLIDAY || CommitmentTypeEnum.VACATION:
        case task.taskTypeEnum.HOLIDAY:
          return `${task.text} (${formattedStartDate} - ${formattedEndDate})`;
        case task.taskTypeEnum.VACATION:
          return `(${formattedStartDate} - ${formattedEndDate}), ${task.text} `;
        case task.taskTypeEnum.TRANSFER:
          return `${task.description} effective from ${formattedStartDate}`;
        case task.taskTypeEnum.TERMINATION:
          return `${task.type} effective from ${formattedStartDate}`;
      }

      let allocationTooltipText = task.text + ' (' + formattedStartDate + ' - ' + formattedEndDate + ')';
      const investmentCategory = !!task.investmentCode ? ' - ' + this.getInvestmentCategory(task.investmentCode) : '';
      const caseRoleNameText = !!task.caseRoleName ? ' - ' + task.caseRoleName : '';
      if (!!investmentCategory || !!caseRoleNameText) {
        allocationTooltipText += investmentCategory + caseRoleNameText;
      } else {
        allocationTooltipText = task.text + ' ' + allocation;
      }
      return allocationTooltipText;
    };
  }

  private configureGridColumns() {

    this.ganttInstance.config.columns = [
      {
        name: 'selectAll', sort: false, label: '<input class="resource-gantt-select-all" type="checkbox" name"selectAll" />', width: 30
      },
      {
        name: 'checked', width: 30, template: function (task) {
          const checked = !!task.isChecked ? 'checked' : '';
          if (!task.editable) {
            return '';
          }
          return '<input class="resource-gantt-checkbox-column" type="checkbox" name="test" id="chkbox_' + task.id + '" value="1" ' + checked + '/>';
        }
      },
      {
        name: 'text', label: 'Task name',  width: '*', template: function (task) {
          if (task.commitmentTypeCode === 'C') {
            return '<a href="javascript:void(0)" class="task-title grouping-padding-left">' + task.text + '</a>';
          }else if(!task.commitmentTypeCode) {
            return  task.text;
          }else {
            return '<span class="grouping-padding-left">' + task.text + '</span>';
          }
        }
      },
      {
        label: 'Allocation%', align: 'center', name: 'allocation', width: 70, template: function (task) {
          // tslint:disable-next-line: no-use-before-declare
          return validateAllocationPercentage(task);
        }, editor: {
          type: 'text', map_to: 'allocation', min: 1, max: 999
        }
      },
      {
        name: 'start_date', label: 'Start Date', width: 100, template: function (task) {
          if (!task.commitmentTypeCode) { // dummy parent row
            return '';
          }
          else{
          switch (task.commitmentTypeCode) {
            case task.commitmentTypeEnum.HOLIDAY:
            case task.commitmentTypeEnum.VACATION:
              return '';
              
            default:
              // tslint:disable-next-line: no-use-before-declare
              return validateDates(task, 'startDate');
          }
          }

        }, align: 'center', editor: { type: 'date', map_to: 'start_date' }
      },
      {
        name: 'end_date', label: 'End Date', width: 100, template: function (task) {
          if (!task.commitmentTypeCode) { // dummy parent row
            return '';
          } else {
          switch (task.type) {
            case task.taskTypeEnum.HOLIDAY:
            case task.taskTypeEnum.VACATION:
            case task.taskTypeEnum.TRANSFER:
              return '';
            default:
              // tslint:disable-next-line: no-use-before-declare
              return validateDates(task, 'endDate');
          }
        }

        }, align: 'center', editor: { type: 'date', map_to: 'end_date' }
      },
    ];
  }

  private configureLayout() {
    // Separate Horizontal bar for gantt chart grid and tabular grid
    this.ganttInstance.config.layout = {
      css: 'gantt_container',
      cols: [
        {
          width: 480,
          min_width: 300,
          rows: [
            {
              view: 'grid',
              scrollX: 'gridScroll',
              scrollable: true,
              scrollY: 'scrollVer'
            },
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

  private configureListenerEvents() {
    if (!this.ganttInstance.ext.inlineEditors.checkEvent('onBeforeSave')) {
      this.ganttInstance.ext.inlineEditors.attachEvent('onBeforeSave', function (state) {
        if (state.oldValue.toString() === state.newValue.toString()) {
          return false;
        }
        return true;
      });
    }

    if (!this.ganttInstance.ext.inlineEditors.checkEvent('onEditStart')) {
      this.ganttInstance.ext.inlineEditors.attachEvent('onEditStart', function (state) {
      });
    }
  }

  private configureGanttBasicProperties() {
    this.ganttInstance.config.sort = true;
    this.ganttInstance.config.readonly = true;
    this.ganttInstance.config.date_grid = '%d-%M-%Y'; // show dates in bain standard dd-MMM-YYYY format
    this.ganttInstance.config.drag_links = false; // prevent linking of items in gantt
    this.ganttInstance.config.inline_editors_date_processing = 'keepDates';
    this.ganttInstance.config.round_dnd_dates = false;
  }

  private getTextFromDescription(commitment) {
    const text = (!commitment.description || commitment.description === '')
      ? commitment.type
      : (
        (commitment.commitmentTypeCode === CommitmentTypeEnum.VACATION
          || commitment.commitmentTypeCode === CommitmentTypeEnum.HOLIDAY
          || commitment.type === EmployeeJobChangeStatusEnum.TRANSFER)
          ? `${commitment.description}`
          : `${commitment.type} - ${commitment.description}`
      );
    return text;
  }
  private attachContextMenu() {
    document.addEventListener('mousedown', () => {
      if (document.getElementById('ganttResourceContextMenuId')) {
        document.getElementById('ganttResourceContextMenuId').innerHTML = '';
        this.configureTextToTooltip(null);
      }
    });
    if (document.getElementById('ganttResourceContextMenuId')) {
      document.getElementById('ganttResourceContextMenuId').addEventListener('mousedown', () => {
        event.stopPropagation();
      });
    }


    document.addEventListener('wheel', () => {
      if (document.getElementById('ganttResourceContextMenuId')) {
        document.getElementById('ganttResourceContextMenuId').innerHTML = '';
      }
    });

    let contextMenuTaskId = null;

    this.ganttInstance.attachEvent('onMouseMove', () => {
      if (document.getElementById('ganttResourceContextMenuId')) {
        if (document.getElementById('ganttResourceContextMenuId').innerHTML !== '') {
          this.configureTextToTooltip(contextMenuTaskId);
          return;
        }
      }
      this.configureTextToTooltip(null);
    });
    this.ganttInstance.attachEvent('onContextMenu', function (taskId, linkId, event) {
      if (document.getElementsByClassName('gantt_tooltip').length > 0) {
        document.getElementsByClassName('gantt_tooltip')[0].remove();
      }
      if (taskId) {
        if (!this.getTask(taskId).editable) {
          return '';
        }
        contextMenuTaskId = taskId;
        const ganttChartHeight = (document.getElementsByClassName('resource-gantt-chart')[0] as HTMLElement).offsetHeight;
        const x = event.clientX + document.body.scrollLeft + document.documentElement.scrollLeft;
        const y = (event.clientY > (ganttChartHeight - 60) ? '80%' : event.clientY + 'px');
        document.getElementById('ganttResourceContextMenuId').innerHTML =
          '<ul id=\'gantt-resource-context-menu-ul\' style=\'top:' + y + ' !important;left:' + x + 'px !important\'></ul>';
        const allocations = this.selectedAllocations.length < 1 ? [this.getTask(taskId)] : this.selectedAllocations;
        // split allocation menu option definition
        this.addContextMenuItem('Split Allocation', allocations, 'Split');

        // delete menu option definition
        this.addContextMenuItem('Delete', allocations, 'Delete');

        // update start date menu option definition
        this.addContextMenuItem('Update Start Date', allocations, 'Start');

        // update end date menu option definition
        this.addContextMenuItem('Update End Date', allocations, 'End');
      }
    });
  }
}
// START: clousres definition for gantt's template functions
function allocationPercentageValidator() {
  return function (task) {
    // NOTE: till now we're only allowing allocation percentage to be editable for case/opporutnity
    // below check should be updated if other allocation types started accepting allocation percentage
    const invlidDiv = '<div id="' + task.id + '" style="background-color: lightcoral;width: inherit; height: inherit">' +
      task.allocation + '</div>';
    const validDiv = '<div id="' + task.id + '">' + task.allocation + '</div>';
    if (task.type === ProjectTypeEnum.CASE || task.type === ProjectTypeEnum.OPP
      || task.commitmentTypeCode === CommitmentTypeEnum.PEG
      || task.commitmentTypeCode === CommitmentTypeEnum.PEG_Surge
      || task.commitmentTypeCode === CommitmentTypeEnum.FRWD
      || task.commitmentTypeCode === CommitmentTypeEnum.AAG
      || task.commitmentTypeCode === CommitmentTypeEnum.ADAPT) {
      if (task.allocation.toString().trim().length < 1) {
        return '<div id="' + task.id + '" style="background-color: lightcoral;width: inherit; height: inherit">' +
          task.allocation + '</div>';
      } else if (parseInt(task.allocation, 10).toString().length > 3) {
        return invlidDiv;
      } else if (isNaN(task.allocation)) {
        // negative number check
        if (parseInt(task.allocation, 10) < 0) {
          return invlidDiv;
        }
        // alphabets check
        const alphaCharsPattern = new RegExp(/^[0-9]+$/);
        if (!alphaCharsPattern.test(task.allocation) && task.allocation[task.allocation.length - 1] !== '%') {
          return invlidDiv;
        }
        // special characters check
        const specialCharsPattern = new RegExp(/[~`!#$\^&*+=\-\[\]\\';,/{}|\\":<>\?.]/);
        if (specialCharsPattern.test(task.allocation)) {
          return invlidDiv;
        }
        // decimal check
        if (task.allocation.indexOf('.') > -1 && task.allocation !== Math.floor(task.allocation)) {
          return invlidDiv;
        }
        return validDiv;
      } else if (task.allocation.toString().indexOf('-') > -1) {
        return invlidDiv;
      } else if (task.allocation.toString().indexOf('.') > -1) {
        return invlidDiv;
      } else {
        return validDiv;
      }
    } else {
      // to disable editing for this column only
      return '<div id="' + task.id +
        '" nonEditable="true" style="width: 116%;height: 100%;margin-left: -5px !important;"></div>';
    }
  };
}

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
      return '<div id="' + task.id + '">' + bainFormattedEndDate + '</div>';
    }
  };
}

function getGanttTimelineColor() {
  return function (task) {
      switch (task.commitmentTypeCode) {
          case CommitmentTypeEnum.HOLIDAY:
          case CommitmentTypeEnum.VACATION:
          case CommitmentTypeEnum.TRAINING:
          case CommitmentTypeEnum.RECRUITING:
              return GanttTimelineColorEnum.ORANGE;
          case CommitmentTypeEnum.PEG:
          case CommitmentTypeEnum.PEG_Surge:
          case CommitmentTypeEnum.AAG:
          case CommitmentTypeEnum.ADAPT:
          case CommitmentTypeEnum.FRWD:
              return GanttTimelineColorEnum.PURPLE;
          case CommitmentTypeEnum.NOT_AVAILABLE:
          case CommitmentTypeEnum.DOWN_DAY:
          case CommitmentTypeEnum.SHORT_TERM_AVAILABLE:
          case CommitmentTypeEnum.LIMITED_AVAILABILITY:
              return GanttTimelineColorEnum.GREEN;
      }

      switch (task.type) {
          case EmployeeJobChangeStatusEnum.TERMINATION:
              return GanttTimelineColorEnum.RED;
          case EmployeeJobChangeStatusEnum.TRANSFER:
              return GanttTimelineColorEnum.ORANGE;
      }

      if (task.commitmentTypeCode === CommitmentTypeEnum.CASE_OPP) {
          if (task.investmentCode !== null && task.investmentCode !== undefined) {
            return GanttTimelineColorEnum.GREY;
          } else if (task.type === ProjectTypeEnum.OPP) {
            return GanttTimelineColorEnum.YELLOW;
          }
      }
  }
}

const validateAllocationPercentage = allocationPercentageValidator();
const validateDates = allocationDatesValidator();
const getTimelineColor = getGanttTimelineColor();
// END


