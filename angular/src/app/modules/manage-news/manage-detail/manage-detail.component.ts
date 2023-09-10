import { Component, OnInit, Injector } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import * as _ from 'lodash';

@Component({
  selector: 'app-manage-detail',
  templateUrl: './manage-detail.component.html',
  styleUrls: ['./manage-detail.component.css']
})
export class ManageDetailComponent  implements OnInit {
 id:number = null;

  constructor(
    private route: ActivatedRoute  
  ) { }
  
  ngOnInit() {
    this.id = this.route.snapshot.params['id'] ? this.route.snapshot.params['id'] : null
  } 
  
}


