import { Injectable } from "@angular/core";
import { ApiGatewayService } from "./api-gateway.service";
import { Retrospective } from "../interfaces/retrospective.interface";
import { BehaviorSubject, filter } from "rxjs";
import { FormGroup } from "@angular/forms";

@Injectable({
    providedIn: 'root'
})
export class RetrospectivesService{

    private getRetrospectives_BSubject = new BehaviorSubject<number>(0);
    private setRetroHubReceiver_BSubject = new BehaviorSubject<boolean>(false);
    showRetrospective_BSubject = new BehaviorSubject<{
        groupName:string,
        sprintId:string,
        username:string,
        sprintName:string
    } | null>(null);
    
    getRetros(sprintNumber:number){
        this.getRetrospectives_BSubject.next(sprintNumber);
    }

    setRetrosSubjectListener(){
        return this.getRetrospectives_BSubject.asObservable().pipe(filter((value) => value !== 0));
    }

    getRetrospectives(groupName:string){
        return this.apiGateway.sendRequest<Retrospective[]>("get", "sprints", `retrospectives/GetRetrospectives/${groupName}`);
    }

    createSprintRetrospective(form:FormGroup){
        return this.apiGateway.sendRequest<any>("post", "sprints", "retrospectives/CreateRetrospective", form.value);
    }

    markFeedbackAsSubmited(username:string, groupName:string, sprintId:string){
        return this.apiGateway.sendRequest<any>("put", "sprints", 
            `feedbacks/MarkFeedbackAsSubmited/${username}?groupName=${groupName}&sprintId=${sprintId}`);
    }

    setRetrosHubReceiver(){
        return this.setRetroHubReceiver_BSubject.next(true);
    }

    getRetrosHubReceiver(){
        return this.setRetroHubReceiver_BSubject.asObservable().pipe(filter((value) => value !== false));
    }

    constructor(private apiGateway:ApiGatewayService){}
}