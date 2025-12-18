import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { environment } from "../../environments/environment";
import { ApiGatewayService } from "./api-gateway.service";

@Injectable({
    providedIn: 'root'
})
export class AssistantService {

    openAssistant(taskContent:string){

        const body = { taskContent };


        return this.apiGateway.sendRequest<any>("post", "task-items", `AskToGemini`, body);
    }

    sendMessage(taskContent:string, prompt:string, previousResponse:string){

        const body = { prompt, previousResponse, taskContent };

        return this.apiGateway.sendRequest<any>("post", "task-items", `AskToGemini`, body)
    }

    constructor(private apiGateway:ApiGatewayService){}

}