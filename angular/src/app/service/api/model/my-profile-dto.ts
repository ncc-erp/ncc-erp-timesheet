export class MyProfileDto{
    id: number;
    fullName : string;
    phone: number;
    birthday: string;
    idCard: string;
    issuedOn: string;
    issuedBy: string;
    placeOfPermanent: string;
    address: string;
    bankinfo: string;
    bankAccountNumber: number;
    remainLeaveDay: number;
    taxCode: string;
    insuranceStatus: number;
    insuranceStatusName: string;
    selected: boolean;
    updatedTime: string;
    updatedUserId:number;
    startWorkingDate: string;
    teamsName?: string;
    email: string;
    sex: number;
    jobPositionId:number;
    team:string;
    status:number;
    level:string;
    branch:string;
    bank:string;
    jobPosition: string;
    avatarFullPath: string;
    userTypeName: string;
    bankId: number;
}
export class ItemInfoDto{
    name: string;
    id:number;
    isDefault: Boolean;
}
export class UpdateUserInfoDto{
    email: string;
    phone: number;
    birthday: string;
    bankId: number;
    idCard: string;
    issuedOn: string;
    issuedBy: string;
    placeOfPermanent: string;
    address: string;
    bankAccountNumber: number;
    taxCode: string;
}

export class GetInfoToUpdateProfileDto extends UpdateUserInfoDto{
    requestStatusName: string;
}

export class GetUserAvatarPathDto{
    avatarPath: string;
    avatarFullPath: string;
}