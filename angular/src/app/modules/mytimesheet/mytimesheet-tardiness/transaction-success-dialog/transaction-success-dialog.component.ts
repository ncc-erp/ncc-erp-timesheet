import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { AppConsts } from '@shared/AppConsts';

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
  baseUrl = AppConsts.buildMmnUrl('transactions/')
    

  constructor(
    public dialogRef: MatDialogRef<TransactionSuccessDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: TransactionSuccessDialogData
  ) {}

  close(): void {
    this.dialogRef.close();
  }

}