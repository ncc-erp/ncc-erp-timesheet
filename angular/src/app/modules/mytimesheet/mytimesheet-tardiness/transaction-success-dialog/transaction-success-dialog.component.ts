import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
// MatSnackBar không còn cần thiết nữa

export interface TransactionSuccessDialogData {
  transactionHash?: string;
  amountPaid?: number;
}

@Component({
  selector: 'app-transaction-success-dialog',
  templateUrl: './transaction-success-dialog.component.html',
  styleUrls: ['./transaction-success-dialog.component.css']
})
export class TransactionSuccessDialogComponent {
  transactionExplorerUrl = 'https://dev-mmn.nccsoft.vn/transactions/';

  constructor(
    public dialogRef: MatDialogRef<TransactionSuccessDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: TransactionSuccessDialogData
  ) {}

  close(): void {
    this.dialogRef.close();
  }

}