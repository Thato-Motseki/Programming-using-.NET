# Reflection – EcoCash Mobile Money Console Application

## Introduction

For this practical assessment, I developed a C# console application that simulates an EcoCash mobile money system. The main purpose of the application was to apply the C# programming concepts covered in class to a practical real-world scenario.

The application allows a user to log in using an EcoCash phone number and PIN, check their balance, send money, deposit money, cash out, view transaction history, and change their PIN.

## What I Learned

One of the main things I learned from this project was how different C# concepts can work together in one application.

I used variables and appropriate data types such as `int` for EcoCash phone numbers, `decimal` for money, and `DateTime` for transaction dates. Using `decimal` for money was particularly important because financial values should not be handled using data types that can cause unnecessary rounding problems.

I also gained a better understanding of Object-Oriented Programming. The `Account` class is responsible for storing customer information and managing account operations, while the `Transaction` class provides a common structure for different types of transactions.

Inheritance was demonstrated through classes such as `SendMoneyTransaction`, `CashOutTransaction`, and `DepositTransaction`. These classes inherit from the `Transaction` class and provide their own implementation of the `Process()` method. This helped me understand how polymorphism can be used to allow different objects to behave differently while sharing a common structure.

## Exception Handling

Exception handling was another important part of the project. The application uses `try-catch` blocks to prevent the program from crashing when invalid information is entered.

For example, if a user enters letters instead of a number when entering a phone number or transaction amount, a `FormatException` is handled by the program.

I also created a custom `InsufficientBalanceException` to handle situations where a user attempts to send or withdraw more money than they have available.

This helped me understand that exception handling is not only about preventing crashes. It can also be used to handle specific problems in a meaningful way.

## File Logging

I implemented a simple logging system using `System.IO`. When an exception occurs, the application records information such as the date, error message, and stack trace in an `errors.log` file.

This was useful because it showed me how applications can keep a record of problems instead of only displaying them to the user.

## Challenges

One of the challenges I experienced was understanding how the different classes should communicate with each other. Initially, it was easy to think about each feature separately, but putting everything together required me to understand how objects could be passed between classes.

Another challenge was implementing exception handling correctly. I had to understand where an error could occur and make sure that the appropriate `catch` block handled it.

I also had to test different situations, including invalid phone numbers, incorrect PINs, insufficient balances, invalid amounts, and successful transactions.

## Testing

I tested the application using both valid and invalid inputs.

Some of the tests included:

* Logging in with a valid EcoCash number and PIN.
* Entering an invalid phone number.
* Entering an incorrect PIN.
* Checking the account balance.
* Sending money to another account.
* Attempting to send more money than the available balance.
* Depositing money.
* Cashing out money.
* Viewing transaction history.
* Changing the account PIN.
* Generating an error log after an exception.

Testing these different scenarios helped me identify problems and understand how the application behaves under different conditions.

## Application of C# Concepts

The project allowed me to demonstrate the main requirements of the assessment:

| Requirement         | Implementation                               |
| ------------------- | -------------------------------------------- |
| Data Types          | `int`, `decimal`, `string`, `DateTime`       |
| Loops               | `while` loop                                 |
| Decision Statements | `if`, `else`, `switch`                       |
| Encapsulation       | Private fields and public methods/properties |
| Abstraction         | Abstract `Transaction` class                 |
| Inheritance         | Transaction subclasses                       |
| Polymorphism        | Overridden `Process()` methods               |
| Collections         | `List<T>` and `Dictionary<TKey, TValue>`     |
| Exception Handling  | `try-catch-finally`                          |
| Custom Exception    | `InsufficientBalanceException`               |
| File Handling       | `File.AppendAllText()`                       |
| Logging             | `errors.log`                                 |

## What I Would Improve

If I were to develop the application further, I would add a database so that accounts and transaction history could be saved permanently. Currently, the sample accounts are created when the program starts, meaning the data does not persist after the application is closed.

I would also improve the transaction reference system so that every transaction has a guaranteed unique reference.

Other possible improvements would include:

* A registration system for new EcoCash users.
* More realistic transaction fees.
* A daily transaction limit.
* Better PIN security.
* A more detailed receipt after transactions.
* An administrator section for managing accounts.
* A graphical user interface instead of a console interface.

However, I intentionally kept the current implementation simple because the purpose of the assessment was to demonstrate fundamental C# and OOP concepts rather than build a production mobile-money platform.

## Conclusion

Developing the EcoCash simulation helped me connect theoretical C# concepts with a practical application. Instead of learning concepts such as inheritance, collections, exception handling, and encapsulation separately, I was able to see how they can be combined to solve a real-world problem.

The project also showed me the importance of testing. A program can compile successfully and still contain problems when users provide unexpected input.

Overall, this assessment improved my understanding of C# programming, Object-Oriented Programming, exception handling, collections, and basic file handling. It also gave me a better understanding of how a simple console application can be structured into multiple classes rather than putting all of the code into one file.
