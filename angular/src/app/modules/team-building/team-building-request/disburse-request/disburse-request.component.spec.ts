import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { DisburseRequestComponent } from './disburse-request.component';

describe('DisburseRequestComponent', () => {
  let component: DisburseRequestComponent;
  let fixture: ComponentFixture<DisburseRequestComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ DisburseRequestComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DisburseRequestComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
