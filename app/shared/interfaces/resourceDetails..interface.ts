import { StaffingHistory } from './staffingHistory.interface';
import { VacationRequest } from './vacationRequest.interface';
import { Employee } from './employee.interface';

export interface ResourceDetails {
  staffingHistory: StaffingHistory;
  vacationRequests: VacationRequest;
  resource: Employee;
}
