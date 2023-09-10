export class UserSalaryMonthDeailDto{
    amount: number;
    description: string;
    id: number;
    userSalaryMonthId: number;
}
export class UserSalaryDto {
    branch: number
    avatarPath: string;
    avatarFullPath: string;
    userName: string;
    surname: string;
    name: string;
    level: string;
    type: number;
    userId: number;
    userSalaryDetails: UserSalaryDetailDto[]
  }
  export class UserSalaryDetailDto {
    amount: number;
    description: string;
    status?: number;
    type : number
    id : number;
    userId : number
  }
export class UserSalaryMonthDto {
    branch: number;
    id: number;
    avatarPath: string;
    avatarFullPath: string;
    userName: string;
    surname: string;
    name: string;
    level: number;
    type: number;
    userId: number;
    salary: number
    allowedLeaveDay: number;
    month: number;
    year: number
    userSalaryDetails: UserSalaryMonthDeailDto[]
  }

