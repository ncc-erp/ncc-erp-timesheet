import { RouterOutlet, ActivationStart, Router } from '@angular/router';
import { Component, OnInit, ViewChild } from '@angular/core';

@Component({
  selector: 'app-main',
  templateUrl: './main.component.html',
  styleUrls: ['./main.component.less']
})
export class MainComponent implements OnInit {
  @ViewChild(RouterOutlet) outlet: RouterOutlet;
  constructor(private router: Router) { }

  ngOnInit() {
    this.router.events.subscribe(e => {
      if (e instanceof ActivationStart && e.snapshot.outlet === 'primary'){
        this.outlet.deactivate();  
      }
    });
  }
}
