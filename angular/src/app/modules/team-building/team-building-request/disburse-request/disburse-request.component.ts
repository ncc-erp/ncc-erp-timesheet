import { Component, Inject, Injector, OnInit, ViewChild } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialog } from '@angular/material';
import { BranchService } from '@app/service/api/branch.service';
import { TeamBuildingRequestService } from '@app/service/api/team-building-request.service';
import { AppComponentBase } from '@shared/app-component-base';
import { NgxStarsComponent } from 'ngx-stars';
import { DisburseDto, DisburseTeamBuildingRequestDto, DisburseTeamBuildingRequestInfoDto, InvoiceDisburseDto } from '../../const/const';
import { FormArray, FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { EditInvoiceMoneyComponent } from './edit-invoice-money/edit-invoice-money.component';
import { request } from 'http';

@Component({
  selector: 'app-disburse-request',
  templateUrl: './disburse-request.component.html',
  styleUrls: ['./disburse-request.component.css']
})
export class DisburseRequestComponent extends AppComponentBase implements OnInit {
  public disburse = {} as DisburseTeamBuildingRequestDto;
  public requestId: number = 0;
  public requesterId: number;
  public requestMoney: number;
  public invoiceAmount: number;
  public saving:boolean = false;
  public isCheckBill:boolean = true;
  public disableSelect: boolean = true;
  public disburseMoney: number = 0;
  public VAT: number = 0;

  public disburseTeambuildingRequestInfoDto : DisburseTeamBuildingRequestInfoDto = new DisburseTeamBuildingRequestInfoDto();

  public disburseDto : DisburseDto = new DisburseDto();
  public invoiceDisburseDto : InvoiceDisburseDto[] = [];

  constructor(
    injector: Injector,
    private teamBuildingRequestService: TeamBuildingRequestService,
    private branchService: BranchService,
    private formBuilder : FormBuilder,
    public dialogRef: MatDialogRef<DisburseRequestComponent>,
    public _dialog: MatDialog,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    super(injector);
  }
  @ViewChild(NgxStarsComponent)
  starsComponent: NgxStarsComponent;

  ngOnInit() {
    this.requestId = this.data.id;
    this.requesterId = this.data.requesterId;
    this.requestMoney = this.data.requestMoney;
    this.invoiceAmount = this.data.invoiceAmount;
    this.VAT = this.getVATConfig();
    this.getDisburseRequestInfo();
  }

  getDisburseRequestInfo() {
    this.teamBuildingRequestService.getTeamBuildingRequestForDisburse(this.requestId, this.requesterId).subscribe((response) => {
      this.disburseTeambuildingRequestInfoDto = response.result;
      this.initDisburseInvoiceList();
      this.disburseMoney = this.calculateTotalSuggestedDisburseMoney();
    }); 
  }

  initDisburseInvoiceList() {
    this.invoiceAmount = 0;
    if(this.disburseTeambuildingRequestInfoDto !== undefined && this.disburseTeambuildingRequestInfoDto !== null) {
      if(this.disburseTeambuildingRequestInfoDto.invoiceRequests !== null) {
        this.disburseTeambuildingRequestInfoDto.invoiceRequests.forEach(item => {
          this.invoiceDisburseDto.push(new InvoiceDisburseDto(item.invoiceId, item.hasVAT));
          this.invoiceAmount += item.invoiceMoney;
        });
      }
    }
  }

  calculateInvoiceAmountAfterTaxing(invoiceMoney : number, hasVAT : boolean) {
    return hasVAT ? invoiceMoney : invoiceMoney + this.calculateVAT(invoiceMoney);
  }

  calculateTotalSuggestedDisburseMoney() {
    let totalVAT = this.calculateTotalVAT();
    if(totalVAT === 0){
      if(this.requestMoney > this.invoiceAmount){
        return this.invoiceAmount;
      }
      else{
        return this.requestMoney;
      }
    }
    else{
      if(this.requestMoney > this.invoiceAmount + totalVAT){
        return this.invoiceAmount;
      }
      else{
        return Math.round(this.requestMoney / (1 + this.VAT));
      }
    }
  }

  calculateVAT(invoiceMoney: number){
    return Math.round(invoiceMoney * this.VAT);
  }
  
  calculateTotalVAT(){
    let totalVAT = 0;
    if(this.disburseTeambuildingRequestInfoDto !== undefined && this.disburseTeambuildingRequestInfoDto !== null 
      && this.disburseTeambuildingRequestInfoDto.invoiceRequests !== undefined && this.disburseTeambuildingRequestInfoDto.invoiceRequests !== null) {
      this.disburseTeambuildingRequestInfoDto.invoiceRequests.forEach(item => {
        if(!item.hasVAT){
          totalVAT += Math.round(item.invoiceMoney * this.VAT);
        }
      })
    }
    return totalVAT;
  }

  close(res): void {
    this.dialogRef.close(res);
  }

  getVATConfig(){
    this.teamBuildingRequestService.getVATConfig().subscribe((rs)=>{
      this.VAT = rs.result / 100;
    })
    return this.VAT;
  }

  onSaveAndClose(){
    this.disburseDto.requestId = this.requestId;
    this.disburseDto.requesterId = this.requesterId;
    this.disburseDto.disburseMoney = this.disburseMoney;
    this.disburseDto.invoiceDisburseList = this.invoiceDisburseDto;

    if(this.disburseDto.disburseMoney >= 0){
      this.teamBuildingRequestService.disburseRequest(this.disburseDto).subscribe(res => {
        this.dialogRef.close(this.disburse);
      });
    } else {
      this.notify.error(this.l("Disburse money required!"));
    }
  }

  editOneInvoice(id: number) {
    const dialogRef = this._dialog.open(EditInvoiceMoneyComponent, {
      disableClose: true,
      width: window.innerWidth >= 600 ? "600px" : "90%",
      data: id,
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        abp.notify.success("Edit invoice money successful");
        this.getDisburseRequestInfo();
      }
    });
  }

  calculateTotalSuggestedRemainingMoney() {
    let totalVAT = this.calculateTotalVAT();
    if(this.disburseTeambuildingRequestInfoDto !== undefined && this.disburseTeambuildingRequestInfoDto !== null
      && this.disburseTeambuildingRequestInfoDto.requestMoney !== undefined && this.disburseMoney > 0){
      if(this.disburseTeambuildingRequestInfoDto.requestMoney <= this.invoiceAmount + totalVAT){
        return 0;
      }
      else {
        return this.requestMoney - this.invoiceAmount - totalVAT;
      }
    }
  }
}

