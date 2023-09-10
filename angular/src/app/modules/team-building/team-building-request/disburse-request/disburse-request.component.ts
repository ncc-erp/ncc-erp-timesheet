import { Component, Inject, Injector, OnInit, ViewChild } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import { BranchService } from '@app/service/api/branch.service';
import { TeamBuildingRequestService } from '@app/service/api/team-building-request.service';
import { AppComponentBase } from '@shared/app-component-base';
import { NgxStarsComponent } from 'ngx-stars';
import { DisburseDto, DisburseTeamBuildingRequestDto, DisburseTeamBuildingRequestInfoDto, InvoiceDisburseDto } from '../../const/const';
import { FormArray, FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';

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
  public billPercentage: number = 0;

  public disburseTeambuildingRequestInfoDto : DisburseTeamBuildingRequestInfoDto = new DisburseTeamBuildingRequestInfoDto();

  public disburseDto : DisburseDto = new DisburseDto();
  public invoiceDisburseDto : InvoiceDisburseDto[] = [];

  constructor(
    injector: Injector,
    private teamBuildingRequestService: TeamBuildingRequestService,
    private branchService: BranchService,
    private formBuilder : FormBuilder,
    public dialogRef: MatDialogRef<DisburseRequestComponent>,
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
    this.getBillPercentageConfig();
    this.getDisburseRequestInfo();
  }

  getDisburseRequestInfo() {
    this.teamBuildingRequestService.getTeamBuildingRequestForDisburse(this.data.id).subscribe((response) => {
      this.disburseTeambuildingRequestInfoDto = response.result;
      this.initDisburseInvoiceList();
    });
  }

  initDisburseInvoiceList() {
    if(this.disburseTeambuildingRequestInfoDto !== undefined && this.disburseTeambuildingRequestInfoDto !== null) {
      if(this.disburseTeambuildingRequestInfoDto.invoiceRequests !== null) {
        this.disburseTeambuildingRequestInfoDto.invoiceRequests.forEach(item => {
          this.invoiceDisburseDto.push(new InvoiceDisburseDto(item.invoiceId, item.hasVAT));
        });
      }
    }
  }

  calculateInvoiceAmountAfterTaxing(invoiceMoney : number, hasVAT : boolean) {
    return hasVAT ? invoiceMoney : invoiceMoney * this.billPercentage;
  }

  updateInvoiveStatusInDisburseInvoiceList(orderNumber : number, hasVAT : boolean) {
    this.invoiceDisburseDto[orderNumber].hasVAT = hasVAT;
  }

  calculateTotalSuggestedDisburseMoney() {
    let suggestedDisburseMoney = 0;
    if(this.disburseTeambuildingRequestInfoDto !== undefined && this.disburseTeambuildingRequestInfoDto !== null) {
      if(this.disburseTeambuildingRequestInfoDto.invoiceRequests !== undefined && this.disburseTeambuildingRequestInfoDto.invoiceRequests !== null) {
        this.disburseTeambuildingRequestInfoDto.invoiceRequests.forEach(item => {
          suggestedDisburseMoney += this.calculateInvoiceAmountAfterTaxing(item.invoiceMoney, item.hasVAT);
        })
      }
    }
    return suggestedDisburseMoney;
  }

  close(res): void {
    this.dialogRef.close(res);
  }

  getBillPercentageConfig(){
    this.teamBuildingRequestService.getBillPercentageConfig().subscribe((rs)=>{
      this.billPercentage = rs.result;
    })
  }

  onSaveAndClose(){
    this.disburseDto.requestId = this.requestId;
    this.disburseDto.requesterId = this.requesterId;
    this.disburseDto.disburseMoney = this.disburseMoney;
    this.disburseDto.invoiceDisburseList = this.invoiceDisburseDto;

    if(this.disburseDto.disburseMoney >= 0){
      this.teamBuildingRequestService.disburseRequest(this.disburseDto).subscribe(res => {
        this.dialogRef.close(this.disburse)
      });
    } else {
      this.notify.error(this.l("Disburse money required!"));
    }
  }
}


