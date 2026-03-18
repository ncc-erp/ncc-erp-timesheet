import { AppComponentBase } from "@shared/app-component-base";
import { WhitelistSystemService } from "../../../service/api/whitelist-system.service";
import { MatDialogRef, MAT_DIALOG_DATA } from "@angular/material";
import { Component, Injector, Inject, OnInit } from "@angular/core";
import { AddWhitelistTypeDto, UpdateWhitelistTypeDto, GetWhitelistSystemDto, WhitelistType } from "@app/service/api/model/whitelist-system.dto";
import { finalize } from "rxjs/operators";
import { Observable } from "rxjs";

@Component({
    selector: 'app-create-edit-whitelist-system',
    templateUrl: './create-edit-whitelist-system.component.html',
    styleUrls: ['./create-edit-whitelist-system.component.css']
})
export class CreateEditWhitelistSystemComponent extends AppComponentBase implements OnInit {
    whitelistItem: any;
    isEdit: boolean = false;
    saving: boolean = false;
    whitelistTypesList: WhitelistType[] = [];

    constructor(
        @Inject(MAT_DIALOG_DATA) public data: GetWhitelistSystemDto,
        injector: Injector,
        private whitelistSystemService: WhitelistSystemService,
        private dialogRef: MatDialogRef<CreateEditWhitelistSystemComponent>
    ) {
        super(injector);
    }
    
    ngOnInit() {
        this.isEdit = this.data.id !== null && this.data.id !== undefined;
        this.whitelistItem = this.data;
        if (!this.isEdit) {
            this.whitelistItem.id = null;
        }
        this.fetchWhitelistTypes();
    }

    fetchWhitelistTypes() {
        this.whitelistSystemService.getWhitelistTypes().subscribe((res: any) => {
            this.whitelistTypesList = res.result ? res.result : res;
        });
    }

    onCodeInput(event: any) {
        this.whitelistItem.code = event.target.value.toUpperCase();
    }

    save(): void {
        this.saving = true;
        let requestObservable: Observable<any>;
        
        if (this.isEdit) {
            const updatePayload = new UpdateWhitelistTypeDto(
                this.whitelistItem.id,
                this.whitelistItem.name,
                this.whitelistItem.code,
                this.whitelistItem.description,
                this.whitelistItem.type,
                this.whitelistItem.isActive
            );
            requestObservable = this.whitelistSystemService.update(updatePayload);
        } else {
            const addPayload = new AddWhitelistTypeDto(
                this.whitelistItem.name,
                this.whitelistItem.code,
                this.whitelistItem.description,
                this.whitelistItem.type,
                this.whitelistItem.isActive
            );
            requestObservable = this.whitelistSystemService.add(addPayload);
        }

        requestObservable.pipe(
            finalize(() => {
                this.saving = false;
            })
        ).subscribe((res) => {
            abp.notify.success(this.l(this.isEdit ? 'Updated successfully' : 'Saved successfully'));
            this.dialogRef.close(true);
        });
    }

    close(): void {
        this.dialogRef.close();
    }
}