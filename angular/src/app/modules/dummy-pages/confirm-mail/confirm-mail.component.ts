import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ConfirmMailService } from '@app/service/api/confirm-mail.service';

@Component({
  selector: 'app-confirm-mail',
  templateUrl: './confirm-mail.component.html',
  styleUrls: ['./confirm-mail.component.css']
})
export class ConfirmMailComponent implements OnInit {
  public message: string = ""
  private payslipId: number

  constructor(private confirmMailService: ConfirmMailService,
    private route: ActivatedRoute) {
      this.payslipId = Number(route.snapshot.queryParamMap.get("id"))
     }

  ngOnInit() {
    this.confirmMail()
  }

  confirmMail() {
    this.confirmMailService.ConfirmPayslipMail(this.payslipId)
      .subscribe(rs => {
        this.message = `<h3>${rs.result}</h3>`
      })
  }
}
