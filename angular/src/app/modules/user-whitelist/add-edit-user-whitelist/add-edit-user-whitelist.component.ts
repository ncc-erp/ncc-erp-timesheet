import { AppComponentBase } from "@shared/app-component-base";
import { UserWhitelistService } from "@app/service/api/user-whitelist.service";
import { MatDialogRef, MAT_DIALOG_DATA } from "@angular/material";
import { Component, Injector, Inject, OnInit } from "@angular/core";
import { AddUserWhitelistDto } from "@app/service/api/model/user-whitelist.dto";
import { finalize } from "rxjs/operators";
import { userDTO } from "@app/modules/user/user.component";
import { UserService } from "@app/service/api/user.service";
import { FormControl } from "@angular/forms";
import { GetWhitelistSystemDto, WhitelistType } from "@app/service/api/model/whitelist-system.dto";
import { WhitelistSystemService } from "@app/service/api/whitelist-system.service";

@Component({
    selector: 'app-add-edit-user-whitelist',
    templateUrl: './add-edit-user-whitelist.component.html',
    styleUrls: ['./add-edit-user-whitelist.component.css']
})
export class AddEditUserWhitelistComponent extends AppComponentBase implements OnInit {
    
    userId: number;
    whitelistName: string;
    whitelistSystems: GetWhitelistSystemDto[] = [];
    selectedWhitelistTypeId: number;
    userSearch: FormControl = new FormControl();
    listUserBase: userDTO[] = [];
    listUserFiltered: userDTO[] = [];

    whitelistTypesList: WhitelistType[] = [];
    saving: boolean = false;

    constructor(
        injector: Injector,
        public dialogRef: MatDialogRef<AddEditUserWhitelistComponent>,
        @Inject(MAT_DIALOG_DATA) public data: any,
        private userService: UserService,
        private userWhitelistService: UserWhitelistService,
        private whitelistSystemService: WhitelistSystemService
    ) {
        super(injector);
    }
    
    ngOnInit(): void {
        this.whitelistTypesList = this.data.whitelistTypesList || [];
        this.getAllUsers();
        this.getAllWhitelistSystems();
        this.userSearch.valueChanges.subscribe(() => {
            this.filterUsers();
        });
    }

    getAllWhitelistSystems() {
        this.whitelistSystemService.getAll().subscribe((rs: any) => {
            this.whitelistSystems = rs.result ? rs.result : rs;
        });
    }

    getAllUsers() {
        this.userService.getAllNotPagging().subscribe(rs => {
            this.listUserBase = rs.result;
            this.listUserFiltered = this.listUserBase;
        });
    }

    filterUsers() {
        if (!this.listUserBase) {
            return;
        }
        let search = this.userSearch.value;
        if (!search) {
            this.listUserFiltered = this.listUserBase;
            return;
        } else {
            search = search.toLowerCase();
        }

        this.listUserFiltered = this.listUserBase.filter(user =>
            (user.userName && user.userName.toLowerCase().indexOf(search) > -1) ||
            (user.emailAddress && user.emailAddress.toLowerCase().indexOf(search) > -1) ||
            (user.fullName && user.fullName.toLowerCase().indexOf(search) > -1)
        );
    }

    save(): void {
        const matchedSystem = this.whitelistSystems.find(
            system => system.type === this.selectedWhitelistTypeId
        );

        if (!matchedSystem) {
            abp.notify.error(this.l('Please add this whitelist type to the system')); 
            return; 
        }

        this.saving = true;
        const input = new AddUserWhitelistDto(
            this.userId,
            matchedSystem.id
        );

        this.userWhitelistService.add(input).pipe(
            finalize(() => {
                this.saving = false;
            })
        ).subscribe(() => {
            abp.notify.success(this.l('Saved successfully'));
            this.dialogRef.close(true);
        });
    }

    close(): void {
        this.dialogRef.close();
    }
}