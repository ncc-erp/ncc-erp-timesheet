export class GetUserWhitelistDto {
    id: number;
    userId: number;
    fullName: string;
    userName: string;
    whitelistName: string;
    whitelistType: number;
}

export class AddUserWhitelistDto {
    userId: number;
    whitelistSystemId: number;
    constructor(userId: number, whitelistSystemId: number) {
        this.userId = userId;
        this.whitelistSystemId = whitelistSystemId;
    }
}