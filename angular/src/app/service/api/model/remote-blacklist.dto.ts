export class AddNewUserToRemoteBlacklistDto {
    userId: number;
    penaltyDays: number;
    constructor(userId: number, penaltyDays: number) {
        this.userId = userId;
        this.penaltyDays = penaltyDays;
    }
}

export class GetRemoteBlacklistDto {
    id: number;
    userId: number;
    fullName: string;
    userName: string;
    penaltyDays: number;
    maxAllowedRemoteDays: number;
}

export class UpdatePenaltyDaysDto {
    id: number;
    penaltyDays: number;
    constructor(id: number, penaltyDays: number) {
        this.id = id;
        this.penaltyDays = penaltyDays;
    }
}

export class ImportRemoteBlacklistResultDto {
    successCount: number;
    failCount: number;
    failedList: string[];
}