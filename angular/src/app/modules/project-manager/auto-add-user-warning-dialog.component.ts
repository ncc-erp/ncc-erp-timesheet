import { Component } from '@angular/core';

@Component({
  selector: 'app-auto-add-user-warning-dialog',
  template: `
    <h2 mat-dialog-title>Warning</h2>
    <mat-dialog-content>
      If you check this box, all users will be added to this project!
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button [mat-dialog-close]="false">Cancel</button>
      <button mat-button color="primary" [mat-dialog-close]="true">OK</button>
    </mat-dialog-actions>
  `
})
export class AutoAddUserWarningDialogComponent {}