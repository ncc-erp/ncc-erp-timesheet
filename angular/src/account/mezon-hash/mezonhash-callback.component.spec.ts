import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { MezonhashCallbackComponent } from './mezonhash-callback.component';

describe('MezonhashCallbackComponent', () => {
  let component: MezonhashCallbackComponent;
  let fixture: ComponentFixture<MezonhashCallbackComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ MezonhashCallbackComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MezonhashCallbackComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
