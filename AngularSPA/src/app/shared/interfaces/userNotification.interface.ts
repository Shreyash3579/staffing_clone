export interface UserNotification {
  notificationId: string;
  description: string;
  notificationDate: Date;
  notificationDueDate: Date;
  notificationStatus: string;
  oldCaseCode?: string;
  employeeCode?: string;
  pipelineId?: string;
}
