import { Component, Inject, Injector, OnInit, ViewChild, ElementRef } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import { AppComponentBase } from '@shared/app-component-base';
import { RemoteBlacklistService } from '@app/service/api/remote-blacklist.service';
import { ImportRemoteBlacklistResultDto } from '@app/service/api/model/remote-blacklist.dto';

@Component({
  selector: 'app-import-csv-dialog',
  templateUrl: './import-csv-dialog.component.html',
  styleUrls: ['./import-csv-dialog.component.css']
})
export class ImportCsvDialogComponent extends AppComponentBase implements OnInit {

  @ViewChild('fileInput') fileInput: ElementRef;
  
  fileName: string = 'Choose file...';
  selectedFile: File | null = null;
  saving: boolean = false;

  constructor(
    injector: Injector,
    public dialogRef: MatDialogRef<ImportCsvDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private remoteBlacklistService: RemoteBlacklistService
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
      const isCsv = file.type === 'text/csv' || file.name.endsWith('.csv');
      const isXlsx = file.type === 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' || file.name.endsWith('.xlsx');
      if (!isCsv && !isXlsx) {
        abp.message.error("Only CSV or XLSX files are allowed!");
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

    this.remoteBlacklistService.importRemoteBlacklist(formData).subscribe(
      (res: any) => {
        this.saving = false;
        const result = res.result ? res.result : res;
        let message = '';
        if (result.failedList && result.failedList.length > 0) {
          const errorDetails = result.failedList.join('<br/>');
          message = `<div style="max-height: 300px; overflow-y: auto;">
                      <b>Successfully imported: ${result.successCount} user(s).</b><br/>
                      <span class="text-danger">Failed: ${result.failCount} record(s).</span><br/><br/>
                      ${errorDetails}
                    </div>`;
          
          abp.message.info(message, "Import Results", true);
          this.dialogRef.close(true);
        } else {
          abp.message.success(`Import ${result.successCount} user(s) successfully!`);
          this.dialogRef.close(true);
        }

        if (result.failCount === 0) {
          this.dialogRef.close(true);
        }
      },
      (err) => {
        this.saving = false;
        abp.message.error(err.message || 'An error occurred during import.');
      }
    );
  }

  close(): void {
    this.dialogRef.close();
  }
}