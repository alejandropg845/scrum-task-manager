import { Injectable, NgZone } from "@angular/core";
import { BackendResponse } from "../interfaces/responses/backend-response.interface";

@Injectable({
    providedIn: 'root'
})
export class PopupService { 

    responses:BackendResponse[] = [];

    showPopup(type: 'e' | 's' | 'i', message:string) {

        const id = Math.random();

        this.ngZone.run(() => {
            
            this.responses.push({ type:type, message, id });

            setTimeout(() => {
                const index = this.responses.findIndex(r => r.id === id);
                this.responses.splice(index, 1);
            }, 2500);
        });

    }

    constructor(private ngZone:NgZone){}

}