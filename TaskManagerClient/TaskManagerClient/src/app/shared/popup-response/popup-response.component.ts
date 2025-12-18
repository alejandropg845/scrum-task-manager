import { Component } from '@angular/core';
import { PopupService } from '../../services/popup.service';
import { animate, style, transition, trigger } from '@angular/animations';

@Component({
  selector: 'app-popup-response',
  templateUrl: './popup-response.component.html',
  styles: ``,
  animations: [
    trigger('popupAnimation', [
      transition(':enter', [
        style({opacity: 0, transform: 'translateY(-30px)'}),
        animate('300ms ease-out', style({ opacity:1, transform: 'translateY(0)' }))
      ]),
      transition(':leave', [
        animate('300ms ease-out', style({ opacity: 0, transform: 'translateY(-30px)' }))
      ])
    ])
  ]
})
export class PopupResponseComponent {

  hidePopup(element:HTMLDivElement){
    element.style.display = 'none';
  }

  constructor(public popup:PopupService){}

}
