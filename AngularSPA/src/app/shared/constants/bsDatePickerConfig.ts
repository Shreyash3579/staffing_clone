import { BsDatepickerConfig } from 'ngx-bootstrap/datepicker';

export const BS_DEFAULT_CONFIG : Partial<BsDatepickerConfig> = {
    containerClass: 'theme-red calendar-pop-up calendar-align-middle',
    customTodayClass: 'custom-today-class',
    dateInputFormat: 'DD-MMM-YYYY',
    rangeInputFormat: "DD-MMM-YYYY",
    isAnimated: true,
    showWeekNumbers: false,
    selectFromOtherMonth: true,
    adaptivePosition: true
};