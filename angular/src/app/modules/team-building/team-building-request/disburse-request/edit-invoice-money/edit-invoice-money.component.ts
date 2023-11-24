import { Component, Inject, Injector, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material';
import { EditOneInvoiceAmountDto } from '@app/modules/team-building/const/const';
import { TeamBuildingRequestService } from '@app/service/api/team-building-request.service';
import { AppComponentBase } from '@shared/app-component-base';

@Component({
  selector: 'app-edit-invoice-money',
  templateUrl: './edit-invoice-money.component.html',
})
export class EditInvoiceMoneyComponent extends AppComponentBase implements OnInit {
  public invoiceId: number = 0;
  public invoiceInfo: EditOneInvoiceAmountDto = new EditOneInvoiceAmountDto();
  public invoiceDto: EditOneInvoiceAmountDto = new EditOneInvoiceAmountDto();
  public invoiceAmount: number = 0;
  constructor(
    injector: Injector,
    private teamBuildingRequestService: TeamBuildingRequestService,
    public dialogRef: MatDialogRef<EditInvoiceMoneyComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    super(injector);
   }

  ngOnInit() {
    this.invoiceId = this.data;
    this.getOneInvoice();
  }

  getOneInvoice() {
    this.teamBuildingRequestService.getOneInvoice(this.invoiceId).subscribe((res) => {
      this.invoiceInfo = res.result;
    })
  }

  close(res): void {
    this.dialogRef.close(res);
  }

  onSaveAndClose(){
    this.invoiceDto.id = this.invoiceId;
    this.invoiceDto.invoiceAmount = this.invoiceAmount;

    if(this.invoiceDto.invoiceAmount > 0){
      this.teamBuildingRequestService.editOneInvoice(this.invoiceDto).subscribe(res => {
        this.dialogRef.close(this.invoiceInfo)
      });
    } else {
      this.notify.error(this.l("Invoice money required!"));
    }
  }

}
