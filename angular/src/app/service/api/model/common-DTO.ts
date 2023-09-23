export class FilterRequest {
  includes: string = '';
  filters: string = '';
  sorts: string = '';
  page: any = 1;
  pageSize: any = 10;
}

export class MyTimeSheetDto {
  projectTaskId: number;
  note: string;
  workingTime: number;
  typeOfWork: number;
  isCharged: boolean;
  dateAt: string;
  id: number;
  status: TimeSheetStatus;
  targetUserWorkingTime: number;
  projectTargetUserId: number;
  projectId: number;
}
export class GetTimeSheetDto {
  projectTaskId: any;
  id: number;
  projectName: string;
  customerName: string;
  projectCode: string;
  taskName: string;
  dateAt: string;
  workingTime: number;
  status: TimeSheetStatus;
  note:string;
  typeOfWork: number;
  isCharged: boolean;
  billable: boolean;
  workType: string;
  isTemp: boolean;

}
export enum TimeSheetStatus {
  New = 0,
  Pending = 1,
  Approve = 2,
  Reject = 3
}

export class DayOfWeek {
  name: string;
  dateAt: string;
  timesheets: GetTimeSheetDto[];
  totalTime: number;
}

export class WeekByTask {
  isCharged: boolean;
  status:TimeSheetStatus;
  projectTaskId: number;
  projectCode: string;
  projectName: string;
  taskName: string;
  customerName: string;
  totalTime: number;
  note : string ; 
  monWorkingTime: string;
  tueWorkingTime: string;
  wedWorkingTime: string;
  thuWorkingTime: string;
  friWorkingTime: string;
  satWorkingTime: string;
  sunWorkingTime: string;
  typeOfWork: number;
  idMonday: number;
  idTueday: number;
  idWeday: number;
  idThuday: number;
  idFriday: number;
  idSatday: number;
  idSunday: number;
  isEditable: boolean;
  isEditing: boolean;
  isAddNew :boolean = false;
 
}
export class PTaskDto {
  projectTaskId: number;
  taskName: string;
  billable: boolean;
  isDefault: boolean;

}

export class PTargetUserDto {
  projectTargetUserId: number;
  userName: string
}

export class ProjectIncludingTaskDto {
  id: number;
  projectName: string
  customerName: string
  projectCode: string
  tasks: PTaskDto[];
  targetUsers: PTargetUserDto[];
  projectUserType: number;
  typeOfWork: number;
  note:string;
  listPM: any[];


}
export class MyTimeSheetByWeekDto {
  projectTaskId: number;
  typeOfWork: number;
  isCharged: boolean;
 customerName:string;
  projectCode: string;
  projectName: string;
  taskName: any;
  note : string;

}







