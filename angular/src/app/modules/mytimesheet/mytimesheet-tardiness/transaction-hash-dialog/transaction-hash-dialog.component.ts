import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import { FormControl, Validators } from '@angular/forms';

@Component({
  selector: 'app-transaction-hash-dialog',
  templateUrl: './transaction-hash-dialog.component.html',
  styleUrls: ['./transaction-hash-dialog.component.css']
})
export class TransactionHashDialogComponent {
  transactionHashControl = new FormControl('', [
    Validators.required,
    Validators.minLength(10),
    Validators.maxLength(255)
  ]);

  constructor(
    public dialogRef: MatDialogRef<TransactionHashDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) { }

  onCancel(): void {
    this.dialogRef.close();
  }

  onSubmit(): void {
    if (this.transactionHashControl.valid) {
      this.dialogRef.close(this.transactionHashControl.value);
    }
  }
}
