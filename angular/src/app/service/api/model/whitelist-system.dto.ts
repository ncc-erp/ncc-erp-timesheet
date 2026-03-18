export class GetWhitelistSystemDto {
    id: number;
    name: string;
    code: string;
    description: string;
    type: number;
    isActive: boolean;
}

export class AddWhitelistTypeDto {
    name: string;
    code: string;
    description: string;
    type: number;
    isActive: boolean;
    constructor(name: string, code: string, description: string, type: number, isActive: boolean) {
        this.name = name;
        this.code = code;
        this.description = description;
        this.type = type;
        this.isActive = isActive;
    }
}

export class UpdateWhitelistTypeDto extends AddWhitelistTypeDto {
    id: number;
    constructor(id: number, name: string, code: string, description: string, type: number, isActive: boolean) {
        super(name, code, description, type, isActive);
        this.id = id;
    }
}

export class WhitelistType {
    value: number;
    name: string;
}