import { Component, Inject } from "@angular/core";
import { MatDialogRef, MAT_DIALOG_DATA } from "@angular/material";
import { FormControl, Validators, AbstractControl, ValidationErrors } from "@angular/forms";
import { MmnTestService } from "./tranfer-from-timesheet.service";

@Component({
  selector: "app-tranfer-from-timesheet-dialog",
  templateUrl: "./tranfer-from-timesheet-dialog.component.html",
  styleUrls: ["./tranfer-from-timesheet-dialog.component.css"],
})
export class TranferDialogComponent {
  minAmount: number = 0;

  recipientAddressControl = new FormControl(
    "HsvGsttQ8swfZehVDMDYqRWEMA8CW6AfyLjk3jBwQagY",
    [Validators.required]
  );

  amountControl: FormControl;
  isSubmitting = false;
  errorMessage: string | null = null;
  
  constructor(
    public dialogRef: MatDialogRef<TranferDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private mmnTestService: MmnTestService
  ) {
    this.minAmount = (data && data.minAmount) ? data.minAmount : 0;
    
    const minAmountValidator = (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) {
        return null; 
      }
      const value = parseFloat(control.value);
      if (isNaN(value)) {
        return null; 
      }
      if (value < this.minAmount) {
        return { minAmount: { min: this.minAmount, actual: value } };
      }
      return null;
    };

    this.amountControl = new FormControl("", [
      Validators.required,
      Validators.pattern("^[0-9]+(\\.[0-9]*)?$"),
      minAmountValidator
    ]);
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

    this.isSubmitting = true;
    this.errorMessage = null; 

    const amount = parseFloat(this.amountControl.value);

    this.mmnTestService.transfer(amount).subscribe({
      next: (transferResult) => {
        this.isSubmitting = false;
        this.dialogRef.close(transferResult);
      },
      error: (err) => {
        console.error("Có lỗi xảy ra khi gọi service:", err);
        this.errorMessage =
          err.message || "An unknown error occurred. Please try again.";
        this.isSubmitting = false;
      },
    });
  }
}
