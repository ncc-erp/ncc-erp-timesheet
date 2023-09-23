import { ActivatedRoute } from '@angular/router';
import { Component, OnInit } from '@angular/core';
import { ConfirmMailService } from '@app/service/api/confirm-mail.service';

@Component({
  selector: 'app-complain-mail',
  templateUrl: './complain-mail.component.html',
  styleUrls: ['./complain-mail.component.css']
})
export class ComplainMailComponent implements OnInit {
  private payslipId: number
  public complainMessage: string = ""
  public result: string = ""
  public isSent = false

  constructor(private route: ActivatedRoute,
    private confirmMailService: ConfirmMailService) {
    this.payslipId = Number(route.snapshot.queryParamMap.get("id"))
  }

  ngOnInit() {
  }

  send() {
    this.confirmMailService.ComplainPayslipMail(this.payslipId, this.complainMessage)
      .subscribe(rs => {
        this.isSent = true
        this.result = `<h3>${rs.result}</h3>`
        abp.notify.success("Complain send successful")
      })
  }
}
