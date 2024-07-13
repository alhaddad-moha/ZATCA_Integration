#include <iostream>
#include <map>
#include <string>
#include <vector>
#include <limits>  // Include for numeric_limits

class Interaction {
public:
    std::string date;
    std::string subject;
};

class Customer {
public:
    std::string name;
    std::string contact;
    std::vector<Interaction> interactions;
};

class User {
public:
    std::string username;
    std::string password;
};

class CRM {
private:
    std::map<int, Customer> customers;
    std::map<std::string, User> users;
    User currentUser;
    bool isLoggedIn;

public:
    CRM() : isLoggedIn(false) {
        User mustafaUser;
        mustafaUser.username = "mustafa";
        mustafaUser.password = "1234";
        users["mustafa"] = mustafaUser;
    }

    bool validateLogin(const std::string& username, const std::string& password) {
        return users.find(username) != users.end() && users[username].password == password;
    }

    void login() {
        std::string username;
        std::string password;

        std::cout << "Enter username: ";
        std::cin >> username;
        std::cout << "Enter password: ";
        std::cin >> password;

        if (validateLogin(username, password)) {
            isLoggedIn = true;
            currentUser = users[username];
            std::cout << "Login successful!\n";
            processOptions();
        }
        else {
            std::cout << "Invalid username or password.\n";
        }
    }

    void logout() {
        std::cout << "Logged out. Goodbye, " << currentUser.username << "!\n";
        isLoggedIn = false;
    }

    bool isUserLoggedIn() const {
        return isLoggedIn;
    }

    void processOptions() {
        int choice;
        int id;
        std::string name;
        std::string contact;
        std::string date;
        std::string subject;

        while (isLoggedIn) {
            std::cout << "1. Add customer\n2. Add interaction\n3. Display customer\n4. Generate report\n5. Analyze data\n6. Logout\n7. Exit\n";
            std::cin >> choice;
            std::cin.ignore(std::numeric_limits<std::streamsize>::max(), '\n');  // Clear input buffer

            switch (choice) {
            case 1:
                std::cout << "Enter customer id: ";
                std::cin >> id;
                std::cin.ignore(std::numeric_limits<std::streamsize>::max(), '\n');  // Clear input buffer
                std::cout << "Enter customer name: ";
                std::getline(std::cin, name);
                std::cout << "Enter customer contact: ";
                std::getline(std::cin, contact);
                addCustomer(id, name, contact);
                break;
            case 2:
                std::cout << "Enter customer id: ";
                std::cin >> id;
                std::cin.ignore(std::numeric_limits<std::streamsize>::max(), '\n');  // Clear input buffer
                std::cout << "Enter interaction date: ";
                std::getline(std::cin, date);
                std::cout << "Enter interaction subject: ";
                std::getline(std::cin, subject);
                addInteraction(id, date, subject);
                break;
            case 3:
                std::cout << "Enter customer id: ";
                std::cin >> id;
                displayCustomer(id);
                break;
            case 4:
                std::cout << "Enter customer id: ";
                std::cin >> id;
                generateReport(id);
                break;
            case 5:
                analyzeData();
                break;
            case 6:
                logout();
                break;
            case 7:
                return;
            default:
                std::cout << "Invalid choice. Please try again.\n";
            }
        }
    }

    void addCustomer(int id, const std::string& name, const std::string& contact) {
        Customer customer;
        customer.name = name;
        customer.contact = contact;
        customers[id] = customer;
        std::cout << "Customer added successfully!\n";
    }

    void addInteraction(int id, const std::string& date, const std::string& subject) {
        Interaction interaction;
        interaction.date = date;
        interaction.subject = subject;
        customers[id].interactions.push_back(interaction);
        std::cout << "Interaction added successfully!\n";
    }

    void displayCustomer(int id) {
        if (customers.find(id) != customers.end()) {
            std::cout << "Name: " << customers[id].name << std::endl;
            std::cout << "Contact: " << customers[id].contact << std::endl;
            for (const auto& interaction : customers[id].interactions) {
                std::cout << "Date: " << interaction.date << ", Subject: " << interaction.subject << std::endl;
            }
        }
        else {
            std::cout << "Customer not found." << std::endl;
        }
    }

    void generateReport(int id) {
        if (customers.find(id) != customers.end()) {
            std::cout << "Report for customer " << customers[id].name << ":" << std::endl;
            std::cout << "Number of interactions: " << customers[id].interactions.size() << std::endl;
            for (const auto& interaction : customers[id].interactions) {
                std::cout << "Date: " << interaction.date << ", Subject: " << interaction.subject << std::endl;
            }
        }
        else {
            std::cout << "Customer not found." << std::endl;
        }
    }

    void analyzeData() {
        std::cout << "Total number of customers: " << customers.size() << std::endl;
        int totalInteractions = 0;
        for (const auto& customer : customers) {
            totalInteractions += customer.second.interactions.size();
        }
        std::cout << "Total number of interactions: " << totalInteractions << std::endl;
    }
};

int main() {
    CRM crm;
    crm.login();

    return 0;
}