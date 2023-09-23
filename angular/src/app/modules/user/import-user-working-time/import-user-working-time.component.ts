import { MatDialogRef } from '@node_modules/@angular/material';
import { MatDialog } from '@angular/material';
import { UserService } from '@app/service/api/user.service';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-import-user-working-time',
  templateUrl: './import-user-working-time.component.html',
  styleUrls: ['./import-user-working-time.component.css']
})
export class ImportUserWorkingTimeComponent implements OnInit {
  selectedFiles: FileList;
  currentFileUpload: File;
  constructor(
    private importService: UserService,
    private dialogRef: MatDialogRef<ImportUserWorkingTimeComponent>
  ) { }

  ngOnInit() {
  }
  selectFile(event) {
    this.selectedFiles = event.target.files;
    this.currentFileUpload = this.selectedFiles.item(0);
  }

  importExcel() {
    if (!this.selectedFiles) {
      abp.message.error("Choose a file!")
      return
    }
    const formData = new FormData();
    formData.append('File', this.currentFileUpload);
    this.importService.ImportUsersFromFile(formData).subscribe(() => {
      abp.message.success("Import successful!");
      this.dialogRef.close("refresh"); 
    }, () => {
    })
  }
}
