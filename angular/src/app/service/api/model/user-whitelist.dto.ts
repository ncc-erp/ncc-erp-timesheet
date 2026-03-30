export class GetUserWhitelistDto {
    id: number;
    userId: number;
    userName: string;
    branch: BranchToDisplayDto;
    projectNames: string[];
    whitelistName: string;
    whitelistType: number;
}

export class BranchToDisplayDto {
    name: string;
    color: string;
}

export class AddUserWhitelistDto {
    userId: number;
    whitelistSystemId: number;
    constructor(userId: number, whitelistSystemId: number) {
        this.userId = userId;
        this.whitelistSystemId = whitelistSystemId;
    }
}