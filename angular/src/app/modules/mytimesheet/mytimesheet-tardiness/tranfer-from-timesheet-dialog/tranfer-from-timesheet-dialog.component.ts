import { Component, Inject, OnInit } from "@angular/core";
import { MatDialogRef, MAT_DIALOG_DATA } from "@angular/material";
import { FormControl, Validators, AbstractControl, ValidationErrors } from "@angular/forms";
import { MmnService } from "./tranfer-from-timesheet.service";
import { mmnClient } from "@shared/mmn-clients"; 

@Component({
  selector: "app-tranfer-from-timesheet-dialog",
  templateUrl: "./tranfer-from-timesheet-dialog.component.html",
  styleUrls: ["./tranfer-from-timesheet-dialog.component.css"],
})
export class TranferDialogComponent implements OnInit {
  readonly SCALE = 1_000_000;

  minAmount: number = 0;
  remainingAmount: number = 0;
  userBalanceScaled: number = 0;    
  userBalanceDisplay: string = '0'; 

  recipientAddressControl: FormControl;

  amountControl: FormControl;
  isSubmitting = false;
  errorMessage: string | null = null;
  
  constructor(
    public dialogRef: MatDialogRef<TranferDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private mmnService: MmnService
  ) {
    const rawMinAmount = (data && data.minAmount) ? data.minAmount : 0;
    this.minAmount = rawMinAmount / this.SCALE; 
    
    const rawRemainingAmount = (data && data.remainingAmount) ? data.remainingAmount : 0;
    this.remainingAmount = rawRemainingAmount; 
    
    const recipientAddress = data && data.donationWallet
      ? data.donationWallet
      : "";
    this.recipientAddressControl = new FormControl(recipientAddress, [
      Validators.required,
    ]);

    const defaultAmount = this.remainingAmount > 0 
      ? this.remainingAmount.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ',')
      : "";
    
    this.amountControl = new FormControl(defaultAmount, [
      Validators.required,
      Validators.pattern("^[0-9,]+(\\.[0-9]*)?$"),
      this.minAmountValidator
    ]);
  }

  ngOnInit(): void {
    this.fetchUserBalance();
  }

  static formatWithCommasAndScale(value: number | string, scale: number = 1_000_000): string {
    if (!value) return '0';
    const num = typeof value === 'string' ? parseFloat(value) : value;
    if (isNaN(num)) return '0';

    const scaled = num / scale;
    return scaled.toLocaleString('en-US', { maximumFractionDigits: 6 }); 
  }

  onAmountInput(event: any) {
    let inputValue = event.target.value;
    if (!inputValue) return;

    
    const rawValue = inputValue.replace(/[^0-9.]/g, ''); 

    if (!rawValue) {
        this.amountControl.setValue('');
        return;
    }
    const parts = rawValue.split('.');
    parts[0] = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, ",");
    const formattedValue = parts.slice(0, 2).join('.');
    if (inputValue !== formattedValue) {
      this.amountControl.setValue(formattedValue, { emitEvent: false });
    }
  }

  private parseNumber(value: string | number): number {
    if (!value) return 0;
    if (typeof value === 'number') return value;
    return parseFloat(value.replace(/,/g, ''));
  }

  minAmountValidator = (control: AbstractControl): ValidationErrors | null => {
    if (!control.value) return null; 
    const value = this.parseNumber(control.value); 
    if (isNaN(value)) return null; 
    if (value < this.minAmount) {
      return { minAmount: { min: this.minAmount, actual: value } };
    }
    return null;
  };

  maxAmountValidator = (control: AbstractControl): ValidationErrors | null => {
    if (!control.value) return null;
    const value = this.parseNumber(control.value);
    if (isNaN(value)) return null;

    if (this.userBalanceScaled > 0 && value > this.userBalanceScaled) {
        return { max: { max: this.userBalanceScaled, actual: value } };
    }
    return null;
  }

  async fetchUserBalance() {
    try {
      const userId = localStorage.getItem('mezonUserId'); 
      if (!userId) return;

      const userAccount = await mmnClient.getAccountByUserId(userId);
      
      if (userAccount && userAccount.balance != null) {
        const rawBalance = Number(userAccount.balance);

        this.userBalanceDisplay = TranferDialogComponent.formatWithCommasAndScale(rawBalance, this.SCALE);
        
        this.userBalanceScaled = rawBalance / this.SCALE;

        this.amountControl.setValidators([
          Validators.required,
          Validators.pattern("^[0-9,]+(\\.[0-9]*)?$"),
          this.minAmountValidator,
          this.maxAmountValidator 
        ]);
        this.amountControl.updateValueAndValidity();
      }
    } catch (error) {
      console.error("Error fetching balance:", error);
    }
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onSubmit(): void {
    if (
      this.recipientAddressControl.invalid ||
      this.amountControl.invalid ||
      this.isSubmitting
    ) {
      return;
    }

    const amountInput = this.parseNumber(this.amountControl.value);
    
    if (amountInput > this.userBalanceScaled) {
      this.errorMessage = `Insufficient balance. Available: ${this.userBalanceDisplay}`;
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = null; 

    const transferAmountRaw = amountInput; 
    const recipientAddress = this.recipientAddressControl.value;

    this.mmnService.transfer(transferAmountRaw, recipientAddress).subscribe({
      next: (transferResult) => {
        this.isSubmitting = false;
        this.dialogRef.close(transferResult);
      },
      error: (err) => {
        this.errorMessage = err.message || "An unknown error occurred.";
        this.isSubmitting = false;
      },
    });
  }
}
