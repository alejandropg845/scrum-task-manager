export interface BeginSprintResponse {
    tasksIds        :string[],
    sprintId        :string,
    sprintName      :string,
    remainingTime   :string,
    expirationTime  :Date,
}