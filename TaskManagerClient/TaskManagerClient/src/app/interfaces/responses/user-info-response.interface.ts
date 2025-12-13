export interface InitialUserInfo {
    username                :string
    isGroupOwner            :boolean,
    groupName               :string,
    avatarBgColor           :string,
    remainingTime           :string,
    groupRole               :string,
    isScrum                 :boolean,
    isAddingTasksAllowed    :boolean,
    expirationTime          :Date,
    status                  :string,
    sprintNumber            :number,
    sprintName              :string,
    finishedSprintName      :string,
    finishedSprintId        :string
}
