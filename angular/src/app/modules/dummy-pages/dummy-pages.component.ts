import { Component, OnInit } from '@angular/core';
import { AppConsts } from '@shared/AppConsts';

@Component({
  selector: 'app-dummy-pages',
  templateUrl: './dummy-pages.component.html',
  styleUrls: ['./dummy-pages.component.css']
})
export class DummyPagesComponent implements OnInit {
  hrEmail:string = ""

  constructor() {
    this.hrEmail = AppConsts.hrEmailAddress
   }

  ngOnInit() {
  }

}
