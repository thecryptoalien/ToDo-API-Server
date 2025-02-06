import requests
import time

#BASE_URL = "http://localhost:8080"  # docker port on localhost
BASE_URL = "https://danmanthenomad.com/ToDo-API-Server"  # remote url on danmanthenomad.com
ENDPOINT_URL = BASE_URL + "/ToDoEntries"

print("Testing ToDo-API-Server endpoints - Part 1")

# register and login object
registerAndLogin = {
    "email": "string@email.com",
    "password": "stringPass123!"
}

# try to register user
print("Trying to Register User...")
registerResponse = requests.post(BASE_URL + "/register", json=registerAndLogin)
print("Status Code", registerResponse.status_code)
if (registerResponse.status_code != 200):
    print("JSON Register Response ", registerResponse.json())

# try to login user
print("Trying to Login User...")
loginResponse = requests.post(BASE_URL + "/login", json=registerAndLogin)
print("Status Code", loginResponse.status_code)
print("JSON login Response ", loginResponse.json())

# set starttime
startTime = time.time()
# get access and refresh tokens 
accessToken = loginResponse.json()["accessToken"]
print("API Access Token ", accessToken)
refreshToken = loginResponse.json()["refreshToken"]
print("API Refresh Token ", refreshToken)

# create headers with Authorization
headers = {
    "Accept": "application/json",
    "Authorization": "Bearer " + accessToken
}

# try ToDoEntries GET 
print("Trying ToDoEntries GET...")
getResponse = requests.get(ENDPOINT_URL, headers=headers)
print("Status Code", getResponse.status_code)
print("JSON ToDoEntries GET Response ", getResponse.json())

# ToDoEntry test object
toDoEntryObject = {
    "title": "ToDoEntry Test Object",
    "description": "This is a test ToDoEntry for python tester",
    "status": 0 # 0 = ToDo
}

# try ToDoEntries POST
postResponse = requests.post(ENDPOINT_URL, headers=headers, json=toDoEntryObject)
print("Status Code", postResponse.status_code)
print("JSON ToDoEntries POST Response ", postResponse.json())

# create parameter object
toDoEntryId = postResponse.json()["id"]
params = {
    "id": toDoEntryId
}
print("Parameters ", params)

# try ToDoEntries PUT
toDoEntryToUpdate = postResponse.json()
toDoEntryToUpdate["status"] = 2
putResponse = requests.put(ENDPOINT_URL + "/" + toDoEntryId, headers=headers, params=params, json=toDoEntryToUpdate)
print("Status Code", putResponse.status_code)

# try ToDoEntries GET with Id param
getWithIdResponse = requests.get(ENDPOINT_URL + "/" + toDoEntryId, headers=headers, params=params)
print("Status Code", getWithIdResponse.status_code)
print("JSON ToDoEntries GET with Id Response ", getWithIdResponse.json())

# try ToDoEntries confirm PUT
confirmParams = {
    "id": toDoEntryId,
    "confirm": True
}
confirmPutResponse = requests.put(ENDPOINT_URL + "/" + toDoEntryId, headers=headers, params=confirmParams)
print("Status Code", confirmPutResponse.status_code)

# try ToDoEntries DELETE
deleteResponse = requests.delete(ENDPOINT_URL + "/" + toDoEntryId, headers=headers, params=params)
print("Status Code", deleteResponse.status_code)

# end of initial tests - moving on
print("Test Part 1 - Time taken in seconds", time.time() - startTime)

# test ratelimiting
print("Starting Part 2 - RateLimit Testing...")
rateTestStart = time.time()
limit = 30
timeout = 60
passes = 5
done = 0
while done < passes:
    for x in range(limit + 1):
        # use base GET for testing
        getRateResponse = requests.get(ENDPOINT_URL, headers=headers)
        if (getRateResponse.status_code != 200):
            print("Status Code", getRateResponse.status_code)
            print("Attempt", x)
            sleepTime = (timeout * (done + 1))  - (time.time() - rateTestStart) + 0.1
            print("Sleeping for", sleepTime)
            time.sleep(sleepTime)
    done = done + 1
print("Test Part 2 - Time taken in seconds", time.time() - rateTestStart)

# test expiry of accessToken with refresh - may take too long so leaving out 3600 seconds is an hour lol

# tests complete
print("Tests Complete - Time taken in seconds", time.time() - startTime)



