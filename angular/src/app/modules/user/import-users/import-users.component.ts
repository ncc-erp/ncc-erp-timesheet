import { Component, Inject, Injector, OnInit, ViewChild, ElementRef } from '@angular/core';
import { MatDialogRef } from '@angular/material';
import { AppComponentBase } from '@shared/app-component-base';
import { UserService } from '@app/service/api/user.service';

@Component({
    selector: 'app-import-users',
    templateUrl: './import-users.component.html',
    styleUrls: ['./import-users.component.css']
})
export class ImportUsersComponent extends AppComponentBase implements OnInit {

    @ViewChild('fileInput') fileInput: ElementRef;

    fileName: string = 'Choose file...';
    selectedFile: File | null = null;
    saving: boolean = false;

    constructor(
        injector: Injector,
        public dialogRef: MatDialogRef<ImportUsersComponent>,
        private userService: UserService
    ) {
        super(injector);
    }

    ngOnInit(): void {
        
    }

    triggerFileInput(): void {
        if (this.fileInput && this.fileInput.nativeElement) {
            this.fileInput.nativeElement.click();
        }
    }

    onFileSelected(event: any): void {
        const file = event.target.files[0];
        if (file) {
            const isXlsx = file.type === 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' || file.name.endsWith('.xlsx');
            if (!isXlsx) {
                abp.message.error("Only XLSX files are allowed!");
                return;
            }
            this.selectedFile = file;
            this.fileName = file.name;
        }
    }

    save(): void {
        if (!this.selectedFile) {
            abp.message.error("Please choose a file!");
            return;
        }
        this.saving = true;
        const formData = new FormData();
        formData.append('file', this.selectedFile);

        this.userService.import(formData).subscribe(
            (res: any) => {
                this.saving = false;
                const result = res.result ? res.result : res;
                const successCount = result.successList ? result.successList.length : 0;
                const failCount = result.failedList ? result.failedList.length : 0;
                
                let message = '';
                if (failCount > 0) {
                    const errorDetails = result.failedList.join('<br/>');
                    message = `<div style="max-height: 300px; overflow-y: auto;"
                                <b>Successfully imported: ${successCount} user(s).</b><br/>
                                <span class="text-danger">Failed: ${failCount} record(s).</span><br/><br/>
                                ${errorDetails}
                                </div>`;

                    abp.message.info(message, "Import Result", true);
                } else {
                    abp.message.success(`Import ${successCount} user(s) successfully!`);
                }

                this.dialogRef.close(true);
            },
            () => {
                this.saving = false;
            }
        )
    }

    close(): void {
        this.dialogRef.close();
    }
}